using Ed.Bannerboard.Models;
using Ed.Bannerboard.UI.Models;
using Ed.Bannerboard.UI.Widgets;
using Microsoft.AspNetCore.Components;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Ed.Bannerboard.UI.Pages
{
    public partial class Index : ComponentBase
    {
        private readonly CancellationTokenSource _disposalTokenSource = new CancellationTokenSource();
        private readonly ClientWebSocket _webSocket = new ClientWebSocket();

        // TODO: Dynamically render and update the widgets from a list when Blazor supports it
        private KingdomStrength kingdomStrength;
        private KingdomLords kingdomLords;
        private KingdomWars kingdomWars;
        private Stats stats;

        protected override async Task OnInitializedAsync()
        {
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
            var buffer = new ArraySegment<byte>(new byte[2048]);
            object receivedObject;
            var statsModel = new StatsModel();

            do
            {
                WebSocketReceiveResult result;
                using (var stream = new MemoryStream())
                {
                    do
                    {
                        // Wait for a message from the server
                        // Read it into the stream via the buffer when one is received
                        result = await _webSocket.ReceiveAsync(buffer, _disposalTokenSource.Token);
                        stream.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _webSocket.Abort();
                        StateHasChanged();
                        break;
                    }

                    // Deserialize the stream into an object
                    // Only expecting messages with binary data
                    stream.Seek(0, SeekOrigin.Begin);
                    var formatter = new BinaryFormatter();
                    receivedObject = formatter.Deserialize(stream);

                    statsModel.LastMessageBytes = stream.Length;
                    statsModel.ReceivedMessageCount++;
                }

                if (receivedObject is HandshakeModel handshake)
                {
                    statsModel.ModVersion = handshake.Version;
                }

                await stats.Update(statsModel);

                // Send model to widgets
                // TODO: Send to the widget that cares, need an array and dynamic rendering to do this

                if (kingdomStrength.CanUpdate(receivedObject, statsModel.ModVersion))
                {
                    await kingdomStrength.Update(receivedObject);
                    continue;
                }

                if (kingdomLords.CanUpdate(receivedObject, statsModel.ModVersion))
                {
                    await kingdomLords.Update(receivedObject);
                    continue;
                }

                if (kingdomWars.CanUpdate(receivedObject, statsModel.ModVersion))
                {
                    await kingdomWars.Update(receivedObject);
                    continue;
                }
            } while (!_disposalTokenSource.IsCancellationRequested);
        }

        public void Dispose()
        {
            _disposalTokenSource.Cancel();
            _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bannerboard client stopped", CancellationToken.None);
        }
    }
}
