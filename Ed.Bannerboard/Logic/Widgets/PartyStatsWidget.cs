using Ed.Bannerboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// A widget for displaying party statistics.
    /// </summary>
    public class PartyStatsWidget : WidgetBase
    {
        /// <summary>
        /// A widget for displaying party statistics.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        /// <param name="version">Mod version.</param>
        public PartyStatsWidget(WebSocketServer server, Version version)
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
            try
            {
                var roster = Campaign.Current.MainParty.MemberRoster.GetTroopRoster();
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
                                Count = roster
                                    .Where(t => t.Character.IsInfantry())
                                    .Sum(t => t.Number),
                                WoundedCount = roster
                                    .Where(t => t.Character.IsInfantry())
                                    .Sum(t => t.WoundedNumber),
                                IsInfantry = true,
                            },
                            new MemberStatsItem
                            {
                                Description = "Archers",
                                Count = roster
                                    .Where(t => t.Character.IsArcher())
                                    .Sum(t => t.Number),
                                WoundedCount = roster
                                    .Where(t => t.Character.IsArcher())
                                    .Sum(t => t.WoundedNumber),
                                IsArcher = true,
                            },
                            new MemberStatsItem
                            {
                                Description = "Cavalry",
                                Count = roster
                                    .Where(t => t.Character.IsCavalry())
                                    .Sum(t => t.Number),
                                WoundedCount = roster
                                    .Where(t => t.Character.IsCavalry())
                                    .Sum(t => t.WoundedNumber),
                                IsCavalry = true,
                            },
                            new MemberStatsItem
                            {
                                Description = "Mounted Archers",
                                Count = roster
                                    .Where(t => t.Character.IsMountedArcher())
                                    .Sum(t => t.Number),
                                WoundedCount = roster
                                    .Where(t => t.Character.IsMountedArcher())
                                    .Sum(t => t.WoundedNumber),
                                IsMountedArcher = true,
                            },
                        },
                    },
                    Version = Version,
                };

                session.Send(model.ToJsonArraySegment());
            }
            catch
            {
                // Ignore
            }
        }
    }
}
