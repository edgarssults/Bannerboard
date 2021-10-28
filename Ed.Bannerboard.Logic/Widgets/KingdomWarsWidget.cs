using Ed.Bannerboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Ed.Bannerboard.Logic.Widgets
{
    public class KingdomWarsWidget : WidgetBase
    {
        public KingdomWarsWidget(WebSocketServer server) : base(server)
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
                            .Select(f => f.Name.ToString())
                            .ToList(),
                        PrimaryColor = Color.FromUint(k.Color).ToString(),
                        SecondaryColor = Color.FromUint(k.Color2).ToString(),
                    })
                    .ToList(),
            };
            session.Send(new ArraySegment<byte>(model.ToByteArray()));
        }
    }
}
