using Ed.Bannerboard.Models;
using Ed.Bannerboard.Models.Widgets;
using Ed.Bannerboard.UI.Models;
using Ed.Bannerboard.UI.Widgets;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ed.Bannerboard.UI.Pages
{
    public partial class Index : ComponentBase
    {
        private readonly CancellationTokenSource _disposalTokenSource = new();
        private readonly ClientWebSocket _webSocket = new();

        // TODO: Dynamically render and update the widgets from a list when Blazor supports it
        private KingdomStrength kingdomStrength;
        private KingdomLords kingdomLords;
        private KingdomWars kingdomWars;
        private Stats stats;
        private StatsModel statsModel;
        private DashboardSettings settings;

        protected override async Task OnInitializedAsync()
        {
            settings = _configuration.GetSection("DashboardSettings").Get<DashboardSettings>();
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
            var buffer = new ArraySegment<byte>(new byte[2048]);

            do
            {
                var receivedText = string.Empty;

                using (var stream = new MemoryStream())
                {
                    WebSocketReceiveResult result;

                    do
                    {
                        // Wait for a message from the server
                        // Read it into the stream via the buffer when one is received
                        Debug.WriteLine("Receiving...");
                        result = await _webSocket.ReceiveAsync(buffer, _disposalTokenSource.Token);
                        stream.Write(buffer.Array, buffer.Offset, result.Count);
                        Debug.WriteLine("Received");
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _webSocket.Abort();
                        StateHasChanged();
                        break;
                    }

                    statsModel.LastMessageBytes = stream.Length;
                    statsModel.ReceivedMessageCount++;

                    // Read the stream as a string
                    // Expecting JSON messages only
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        receivedText = await reader.ReadToEndAsync();
                        Debug.WriteLine("Message: " + receivedText);
                    }
                }

                Debug.WriteLine("Matching...");
                if (Regex.IsMatch(receivedText, "\"Type\":.*\"HandshakeModel\""))
                {
                    Debug.WriteLine("Matched HandshakeModel");
                    var model = JsonConvert.DeserializeObject<HandshakeModel>(receivedText, new VersionConverter());
                    Debug.WriteLine("Deserialized HandshakeModel");
                    statsModel.ModVersion = model.Version;
                    Debug.WriteLine("Set version");
                }

                await stats.Update(statsModel);

                // Send model to widgets
                // TODO: Send to the widget that cares, need an array and dynamic rendering to do this

                if (Regex.IsMatch(receivedText, "\"Type\":.*\"KingdomStrengthModel\""))
                {
                    Debug.WriteLine("Matched KingdomStrengthModel");
                    var model = JsonConvert.DeserializeObject<KingdomStrengthModel>(receivedText, new VersionConverter());
                    if (kingdomStrength.CanUpdate(model, statsModel.ModVersion))
                    {
                        await kingdomStrength.Update(model);
                        continue;
                    }
                }

                if (Regex.IsMatch(receivedText, "\"Type\":.*\"KingdomLordsModel\""))
                {
                    Debug.WriteLine("Matched KingdomLordsModel");
                    var model = JsonConvert.DeserializeObject<KingdomLordsModel>(receivedText, new VersionConverter());
                    if (kingdomLords.CanUpdate(model, statsModel.ModVersion))
                    {
                        await kingdomLords.Update(model);
                        continue;
                    }
                }

                if (Regex.IsMatch(receivedText, "\"Type\":.*\"KingdomWarsModel\""))
                {
                    Debug.WriteLine("Matched KingdomWarsModel");
                    var model = JsonConvert.DeserializeObject<KingdomWarsModel>(receivedText, new VersionConverter());
                    if (kingdomWars.CanUpdate(model, statsModel.ModVersion))
                    {
                        await kingdomWars.Update(model);
                        continue;
                    }
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
