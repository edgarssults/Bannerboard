using Ed.Bannerlord.Dashboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Ed.Bannerlord.Dashboard.Logic.Widgets
{
    /// <summary>
    /// A widget for displaying kingdom strength charts on the Bannerlord Dashboard.
    /// </summary>
    public class KingdomStrengthWidget : WidgetBase
    {
        private readonly WebSocketServer _server;

        /// <summary>
        /// A widget for displaying kingdom strength charts on the Bannerlord Dashboard.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        public KingdomStrengthWidget(WebSocketServer server)
        {
            _server = server;
        }

        /// <summary>
        /// Registers widget events.
        /// </summary>
        public override void RegisterEvents()
        {
            // Update all sessions about kingdom strength every "hour"
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(() =>
            {
                foreach (var session in _server.GetAllSessions())
                {
                    SendUpdate(session);
                }
            }));
        }

        /// <summary>
        /// Syncs widget save data.
        /// </summary>
        /// <param name="dataStore">Data store.</param>
        public override void SyncData(IDataStore dataStore)
        {
            // Nothing to sync
        }

        /// <summary>
        /// Initializes a widget.
        /// </summary>
        /// <param name="session">The session to initialize the widget for.</param>
        public override void Init(WebSocketSession session)
        {
            // Send the first kingdom strength update
            SendUpdate(session);
        }

        /// <summary>
        /// Sends a dashboard update to a WebSocket session.
        /// </summary>
        /// <param name="session">The session to send the update to.</param>
        private void SendUpdate(WebSocketSession session)
        {
            var model = new KingdomStrengthModel
            {
                Kingdoms = Campaign.Current.Kingdoms.ToDictionary(k => k.Name.ToString(), v => v.TotalStrength),
            };
            session.Send(new ArraySegment<byte>(model.ToByteArray()));
        }
    }
}
