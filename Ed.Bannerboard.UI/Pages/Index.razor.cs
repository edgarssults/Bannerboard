using Blazored.LocalStorage;
using Blazored.Toast.Services;
using Ed.Bannerboard.Models;
using Ed.Bannerboard.UI.Logic;
using Ed.Bannerboard.UI.Models;
using Ed.Bannerboard.UI.Widgets;
using Excubo.Blazor.Grids;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Pages
{
    public partial class Index : ComponentBase
    {
        private const string BannerboardLayoutKey = "bannerboard-layout";
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ClientWebSocket _webSocket = new();
        private readonly List<WidgetComponent> _widgets = new()
        {
            new WidgetComponent(typeof(KingdomStrength), 0, 0, 5, 6),
            new WidgetComponent(typeof(KingdomLords), 0, 6, 5, 6),
            new WidgetComponent(typeof(KingdomWars), 5, 0, 5, 6),
            new WidgetComponent(typeof(PartyStats), 5, 6, 5, 6),
            new WidgetComponent(typeof(HeroTracker), 10, 6, 6, 6),
            new WidgetComponent(typeof(TownProsperity), 10, 0, 6, 6),
            new WidgetComponent(typeof(Stats), 16, 0, 3, 6)
        };

        private StatsModel? _statsModel;
        private bool _modVersionDetermined;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        [Inject]
        private IConfiguration? Configuration { get; set; }

        [Inject]
        private IToastService? ToastService { get; set; }

        [Inject]
        private AppState? AppState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadLayoutFromStorage();
            AppState!.OnResetLayout += OnResetLayout;

            var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
            _statsModel = new StatsModel
            {
                DashboardVersion = new Version(settings!.Version)
            };

            try
            {
                // TODO: Change the port or use an URL?
                await _webSocket.ConnectAsync(new Uri("ws://localhost:2020"), _cancellationTokenSource.Token);
				_ = ReceiveLoopAsync();
			}
            catch
            {
				// TODO: Retry failed connections
				// https://websocket.org/guides/languages/csharp/#reconnecting-websocket-client
			}
		}

        private async Task ReceiveLoopAsync()
        {
			var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
			var messageBuilder = new List<byte>();

			while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
				try
				{
					WebSocketReceiveResult result;

					do
					{
						// Wait for a message from the server and read it fully
						Debug.WriteLine("Receiving...");
						result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);

						if (result.MessageType == WebSocketMessageType.Close)
						{
							_webSocket.Abort();
							StateHasChanged();
							return;
						}

						messageBuilder.AddRange(buffer.Array!.Take(result.Count));
					}
					while (!result.EndOfMessage);

					if (result.MessageType == WebSocketMessageType.Binary)
					{
						Debug.WriteLine("Received, processing...");
						var messageArray = messageBuilder.ToArray();

						_statsModel!.LastMessageBytes = messageArray.Length;
						_statsModel!.ReceivedMessageCount++;

						// Expecting JSON messages only
						var message = Encoding.UTF8.GetString(messageArray);
						Debug.WriteLine(message);

						// Handshake message is sent once
						HandleHandshakeMessage(message);

						// Stats always updated
						await UpdateWidgets(JsonConvert.SerializeObject(_statsModel));

						// Update the relevant widget
						await UpdateWidgets(message);
					}

					messageBuilder.Clear();
				}
				catch (OperationCanceledException)
				{
					Debug.WriteLine("Receive loop cancelled");
					break;
				}
				catch (WebSocketException ex)
				{
					Debug.WriteLine($"WebSocket error: {ex.Message}");
					ToastService?.ShowError("Connection to Bannerboard server lost, refresh the page.", settings => settings.DisableTimeout = true);
					break;
				}
			}

			Debug.WriteLine("Receive loop exited");
		}

        private void HandleHandshakeMessage(string message)
        {
            if (_modVersionDetermined)
            {
                return;
            }

            if (!Regex.IsMatch(message, "\"Type\":.*\"HandshakeModel\""))
            {
                return;
            }

            var model = JsonConvert.DeserializeObject<HandshakeModel>(message, new VersionConverter());
            _statsModel!.ModVersion = model?.Version;
            _modVersionDetermined = true;

            // Widgets are now rendered and instances are available
            // Because they are only rendered once the WS connection is open
            // Widgets can now start sending messages to the server
            SubscribeToWidgetMessageSent();

			Debug.WriteLine("Handshake processed");
		}

        private async Task UpdateWidgets(string message)
        {
            foreach (var widget in _widgets)
            {
                if (widget.Component?.Instance is not IWidget widgetInstance)
                {
                    continue;
                }

                if (!widgetInstance.CanUpdate(message, _statsModel?.ModVersion))
                {
                    continue;
                }

				try
				{
					await widgetInstance.Update(message);
					Debug.WriteLine("Widget updated: " + widgetInstance.GetType().Name);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Error updating widget " + widgetInstance.GetType().Name + ": " + ex.Message);
				}

				// Widget found and updated, no need to continue
				return;
            }
		}

        private void SubscribeToWidgetMessageSent()
        {
			foreach (var widget in _widgets)
			{
				if (widget.Component?.Instance is not IWidget widgetInstance)
				{
					continue;
				}

				widgetInstance.MessageSent += OnMessageSent;
				widgetInstance.SendInitialMessage();
			}

			Debug.WriteLine("Widget message sending enabled");
		}

        private async Task LoadLayoutFromStorage()
        {
            var layout = await LocalStorage!.GetItemAsync<List<WidgetLayout>>(BannerboardLayoutKey);
            if (layout == null)
            {
                // No layout stored, defaults will be used
                return;
            }

            if (!_widgets.All(w => layout.Any(l => l.Type == w.Type.Name)))
            {
                // Stored widgets are different than available widgets, should reset layout
                await LocalStorage!.RemoveItemAsync(BannerboardLayoutKey);
                ToastService?.ShowInfo("Stored layout has been reset due to widget changes.");
                return;
            }

            _widgets.ForEach(w =>
            {
                var storedLayout = layout.FirstOrDefault(l => l.Type == w.Type.Name);
                if (storedLayout == null)
                {
                    // Widget doesn't have a layout stored, this shouldn't really happen
                    return;
                }

                w.Column = storedLayout.Column;
                w.Row = storedLayout.Row;
                w.ColumnSpan = storedLayout.ColumnSpan;
                w.RowSpan = storedLayout.RowSpan;
            });
        }

        private async void OnResize(ElementResizeArgs args)
        {
            await LocalStorage!.SetItemAsync(BannerboardLayoutKey, _widgets.ToWidgetLayout());
        }

        private async void OnMove(ElementMoveArgs args)
        {
            await LocalStorage!.SetItemAsync(BannerboardLayoutKey, _widgets.ToWidgetLayout());
        }

        private async void OnResetLayout()
        {
            await LocalStorage!.RemoveItemAsync(BannerboardLayoutKey);

			_widgets.ForEach(w =>
            {
                w.Column = w.DefaultColumn;
                w.Row = w.DefaultRow;
                w.ColumnSpan = w.DefaultColumnSpan;
                w.RowSpan = w.DefaultRowSpan;

				if (w.Component?.Instance is IWidget widgetInstance)
				{
					widgetInstance.ResetAsync();
				}
			});

            StateHasChanged();
        }

		private async void OnMessageSent(object? sender, string message)
		{
			try
			{
				var encoded = Encoding.UTF8.GetBytes(message);
				var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
				await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
			}
			catch (OperationCanceledException)
			{
				Debug.WriteLine("Message sending interrupted");
			}
		}

        public void Dispose()
        {
			_cancellationTokenSource.Cancel();
            _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bannerboard client stopped", CancellationToken.None);
        }
    }
}
