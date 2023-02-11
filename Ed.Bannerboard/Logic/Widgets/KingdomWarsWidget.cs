using Ed.Bannerboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// A widget for displaying a list of wars each kingdom is in.
    /// </summary>
    public class KingdomWarsWidget : WidgetBase
    {
        /// <summary>
        /// A widget for displaying a list of wars each kingdom is in.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        /// <param name="version">Mod version.</param>
        public KingdomWarsWidget(WebSocketServer server, Version version)
            : base(server, version)
        {
        }

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

        public override void Init(WebSocketSession session)
        {
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
            var model = new KingdomWarsModel
            {
                Kingdoms = Campaign.Current.Kingdoms
                    .Select(k => new KingdomWarsItem
                    {
                        Name = k.Name.ToString(),
                        Wars = Campaign.Current.Factions
                            .Where(f => k.IsAtWarWith(f) && (f.IsKingdomFaction || f.IsMinorFaction))
                            .Select(f => new KingdomWarsFactionItem
                            {
                                Name = f.Name.ToString(),
                                IsKingdomFaction = f.IsKingdomFaction,
                                IsMinorFaction = f.IsMinorFaction
                            })
                            .ToList(),
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
