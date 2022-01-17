using Ed.Bannerboard.Models;
using Ed.Bannerboard.UI.Logic;
using Ed.Bannerboard.UI.Models;
using Ed.Bannerboard.UI.Widgets;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Pages
{
    public partial class Index : ComponentBase
    {
        private readonly CancellationTokenSource _disposalTokenSource = new();
        private readonly ClientWebSocket _webSocket = new();
        private readonly List<WidgetComponent> _widgets = new()
        {
            new WidgetComponent(typeof(KingdomStrength)),
            new WidgetComponent(typeof(KingdomLords)),
            new WidgetComponent(typeof(KingdomWars)),
            new WidgetComponent(typeof(Stats))
        };

        private StatsModel? statsModel;
        private bool modVersionDetermined;

        protected override async Task OnInitializedAsync()
        {
            var settings = _configuration.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
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
                GetModVersionFromHandshake(message);

                // Stats always updated
                await UpdateWidgets(JsonConvert.SerializeObject(statsModel));

                // Update the relevant widget
                await UpdateWidgets(message);
            } while (!_disposalTokenSource.IsCancellationRequested);
        }

        private void GetModVersionFromHandshake(string message)
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
        }

        private async Task UpdateWidgets(string message)
        {
            foreach (var widget in _widgets)
            {
                if (widget.Component?.Instance is not IWidget widgetComponent)
                {
                    continue;
                }

                if (widgetComponent.CanUpdate(message, statsModel?.ModVersion))
                {
                    await widgetComponent.Update(message);
                    return;
                }
            }
        }

        public void Dispose()
        {
            _disposalTokenSource.Cancel();
            _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bannerboard client stopped", CancellationToken.None);
        }
    }
}
