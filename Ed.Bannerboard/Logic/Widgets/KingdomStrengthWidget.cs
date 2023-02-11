using Ed.Bannerboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// A widget for displaying kingdom strength charts.
    /// </summary>
    public class KingdomStrengthWidget : WidgetBase
    {
        /// <summary>
        /// A widget for displaying kingdom strength charts.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        /// <param name="version">Mod version.</param>
        public KingdomStrengthWidget(WebSocketServer server, Version version)
            : base(server, version)
        {
        }

        public override void RegisterEvents()
        {
            // Update all sessions about kingdom strength every "hour"
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(() =>
            {
                foreach (var session in Server.GetAllSessions())
                {
                    SendUpdate(session);
                }
            }));
        }

        public override void Init(WebSocketSession session)
        {
            // Send the first update
            SendUpdate(session);
        }

        public override bool CanHandleMessage(string message)
        {
            return false;
        }

        public override void HandleMessage(WebSocketSession session, string message)
        {
            throw new NotImplementedException();
        }

        private void SendUpdate(WebSocketSession session)
        {
            var model = new KingdomStrengthModel
            {
                Kingdoms = Campaign.Current.Kingdoms
                    .Select(k => new KingdomStrengthItem
                    {
                        Name = k.Name.ToString(),
                        Strength = k.TotalStrength,
                        PrimaryColor = Color.FromUint(k.Color).ToString(),
                        SecondaryColor = Color.FromUint(k.Color2).ToString(),
                    })
                    .ToList(),
                Version = Version,
            };

            session.Send(model.ToJsonArraySegment());
        }
    }
}
