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
        private readonly CancellationTokenSource _disposalTokenSource = new();
        private readonly ClientWebSocket _webSocket = new();
        private readonly List<WidgetComponent> _widgets = new()
        {
            new WidgetComponent(typeof(KingdomStrength), 0, 0, 5, 6),
            new WidgetComponent(typeof(KingdomLords), 0, 6, 5, 6),
            new WidgetComponent(typeof(KingdomWars), 5, 0, 5, 6),
            new WidgetComponent(typeof(PartyStats), 5, 6, 5, 6),
            new WidgetComponent(typeof(Stats), 10, 6, 3, 6),
            new WidgetComponent(typeof(TownProsperity), 10, 0, 6, 6),
        };

        private StatsModel? statsModel;
        private bool modVersionDetermined;

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
            statsModel = new StatsModel
            {
                DashboardVersion = new Version(settings.Version)
            };

            try
            {
                // TODO: Change the port or use an URL?
                await _webSocket.ConnectAsync(new Uri("ws://localhost:2020"), _disposalTokenSource.Token);
                _ = ReceiveLoop();
            }
            catch
            {
                // TODO: Retry failed connections
            }
        }

        private async Task ReceiveLoop()
        {
            do
            {
                var buffer = new ArraySegment<byte>(new byte[2048]);
                var message = string.Empty;

                using (var stream = new MemoryStream())
                {
                    WebSocketReceiveResult result;

                    do
                    {
                        // Wait for a message from the server
                        // Read it into the stream via the buffer when one is received
                        Debug.WriteLine("Receiving...");
                        result = await _webSocket.ReceiveAsync(buffer, _disposalTokenSource.Token);
                        stream.Write(buffer.Array!, buffer.Offset, result.Count);
                        Debug.WriteLine("Received, deserializing...");
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _webSocket.Abort();
                        StateHasChanged();
                        break;
                    }

                    statsModel!.LastMessageBytes = stream.Length;
                    statsModel!.ReceivedMessageCount++;

                    // Read the stream as a string
                    // Expecting JSON messages only
                    stream.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(stream);
                    message = await reader.ReadToEndAsync();
                    Debug.WriteLine("Message: " + message);
                }

                // Handshake message is sent once
                HandleHandhsakeMessage(message);

                // Stats always updated
                await UpdateWidgets(JsonConvert.SerializeObject(statsModel));

                // Update the relevant widget
                await UpdateWidgets(message);
            } while (!_disposalTokenSource.IsCancellationRequested);
        }

        private void HandleHandhsakeMessage(string message)
        {
            if (modVersionDetermined)
            {
                return;
            }

            if (!Regex.IsMatch(message, "\"Type\":.*\"HandshakeModel\""))
            {
                return;
            }

            var model = JsonConvert.DeserializeObject<HandshakeModel>(message, new VersionConverter());
            statsModel!.ModVersion = model?.Version;
            modVersionDetermined = true;

            // Widgets are now rendered and instances are available
            // Because they are only rendered once the WS connection is open
            // Widgets can now start sending messages to the server
            SubscribeToWidgetMessageSent();
        }

        private async Task UpdateWidgets(string message)
        {
            foreach (var widget in _widgets)
            {
                if (widget.Component?.Instance is not IWidget widgetInstance)
                {
                    continue;
                }

                if (!widgetInstance.CanUpdate(message, statsModel?.ModVersion))
                {
                    continue;
                }

                await widgetInstance.Update(message);
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

                widgetInstance.MessageSent += async (sender, message) =>
                {
                    var encoded = Encoding.UTF8.GetBytes(message);
                    var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                    await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _disposalTokenSource.Token);
                };

                widgetInstance.SendInitialMessage();
            }
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
            });

            StateHasChanged();
        }

        public void Dispose()
        {
            AppState!.OnResetLayout -= OnResetLayout;
            _disposalTokenSource.Cancel();
            _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bannerboard client stopped", CancellationToken.None);
        }
    }
}
