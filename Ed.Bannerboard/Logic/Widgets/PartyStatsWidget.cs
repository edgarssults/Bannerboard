using Ed.Bannerboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// A widget for displaying party statistics on the dashboard.
    /// </summary>
    public class PartyStatsWidget : WidgetBase
    {
        /// <summary>
        /// A widget for displaying party statistics on the dashboard.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        /// <param name="version">Mod version.</param>
        public PartyStatsWidget(WebSocketServer server, Version version)
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
            var model = new PartyStatsModel
            {
                Food = new FoodStats
                {
                    Items = Campaign.Current.MainParty.ItemRoster
                        .Where(i => i.EquipmentElement.Item.IsFood)
                        .Select(i => new FoodStatsItem
                        {
                            Name = i.EquipmentElement.Item.Name.ToString(),
                            Count = i.Amount
                        })
                        .ToList(),
                },
                Members = new MemberStats
                {
                    TotalHeroes = Campaign.Current.MainParty.MemberRoster.TotalHeroes,
                    TotalRegulars = Campaign.Current.MainParty.MemberRoster.TotalRegulars,
                    WoundedHeroes = Campaign.Current.MainParty.MemberRoster.TotalWoundedHeroes,
                    WoundedRegulars = Campaign.Current.MainParty.MemberRoster.TotalWoundedRegulars,
                    TotalCount = Campaign.Current.MainParty.MemberRoster.TotalManCount,
                    TotalWounded = Campaign.Current.MainParty.MemberRoster.TotalWounded,
                    MaxCount = Campaign.Current.MainParty.LimitedPartySize,
                    Items = new List<MemberStatsItem>
                    {
                        new MemberStatsItem
                        {
                            Description = "Infantry",
                            Count = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => t.Character.IsInfantry && !t.Character.IsRanged && !t.Character.IsMounted)
                                .Sum(t => t.Number),
                            WoundedCount = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => t.Character.IsInfantry && !t.Character.IsRanged && !t.Character.IsMounted)
                                .Sum(t => t.WoundedNumber),
                            IsInfantry = true,
                        },
                        new MemberStatsItem
                        {
                            Description = "Archers",
                            Count = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => !t.Character.IsInfantry && t.Character.IsRanged && !t.Character.IsMounted)
                                .Sum(t => t.Number),
                            WoundedCount = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => !t.Character.IsInfantry && t.Character.IsRanged && !t.Character.IsMounted)
                                .Sum(t => t.WoundedNumber),
                            IsArcher = true,
                        },
                        new MemberStatsItem
                        {
                            Description = "Cavalry",
                            Count = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => !t.Character.IsInfantry && !t.Character.IsRanged && t.Character.IsMounted)
                                .Sum(t => t.Number),
                            WoundedCount = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => !t.Character.IsInfantry && !t.Character.IsRanged && t.Character.IsMounted)
                                .Sum(t => t.WoundedNumber),
                            IsCavalry = true,
                        },
                        new MemberStatsItem
                        {
                            Description = "Mounted Archers",
                            Count = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => !t.Character.IsInfantry && t.Character.IsRanged && t.Character.IsMounted)
                                .Sum(t => t.Number),
                            WoundedCount = Campaign.Current.MainParty.MemberRoster.GetTroopRoster()
                                .Where(t => !t.Character.IsInfantry && t.Character.IsRanged && t.Character.IsMounted)
                                .Sum(t => t.WoundedNumber),
                            IsMountedArcher = true,
                        },
                    },
                },
                Version = Version,
            };
            session.Send(model.ToJsonArraySegment());
        }
    }
}
