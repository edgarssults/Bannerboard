using Ed.Bannerboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// A widget for displaying town prosperity on the dashboard.
    /// </summary>
    public class TownProsperity : WidgetBase
    {
        /// <summary>
        /// A widget for displaying town prosperity on the dashboard.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        /// <param name="version">Mod version.</param>
        public TownProsperity(WebSocketServer server, Version version)
            : base(server, version)
        {
        }

        /// <summary>
        /// Registers widget events.
        /// </summary>
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(() =>
            {
                foreach (var session in Server.GetAllSessions())
                {
                    SendUpdate(session);
                }
            }));
        }

        /// <summary>
        /// Initializes a widget.
        /// </summary>
        /// <param name="session">The session to initialize the widget for.</param>
        public override void Init(WebSocketSession session)
        {
            SendUpdate(session);
        }

        /// <summary>
        /// Sends a dashboard update to a WebSocket session.
        /// </summary>
        /// <param name="session">The session to send the update to.</param>
        private void SendUpdate(WebSocketSession session)
        {
            var model = new TownProsperityModel
            {
                Towns = Campaign.Current.Settlements
                    .Where(s => s.IsTown)
                    .OrderByDescending(s => s.Prosperity)
                    .Take(10)
                    .Select(s => new TownProsperityItem
                    {
                        Name = s.Name.ToString(),
                        Prosperity = s.Prosperity,
                        PrimaryColor = Color.FromUint(s.MapFaction.Color).ToString(),
                        SecondaryColor = Color.FromUint(s.MapFaction.Color2).ToString()
                    })
                    .ToList(),
                Version = Version,
            };
            session.Send(model.ToJsonArraySegment());
        }
    }
}
