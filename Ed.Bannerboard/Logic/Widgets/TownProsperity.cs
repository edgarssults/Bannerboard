using Ed.Bannerboard.Models.Widgets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SuperSocket.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// A widget for displaying town prosperity on the dashboard.
    /// </summary>
    public class TownProsperity : WidgetBase
    {
        private int _townCount = 10;

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

        public override void Init(WebSocketSession session)
        {
            // Not sending anything because the client will request how many towns to show
        }

        public override bool CanHandleMessage(string message)
        {
            // Note: Version is not checked yet
            return Regex.IsMatch(message, $"\"Type\":.*\"{nameof(TownProsperityFilterModel)}\"");
        }

        public override void HandleMessage(WebSocketSession session, string message)
        {
            var model = JsonConvert.DeserializeObject<TownProsperityFilterModel>(message, new VersionConverter());
            if (model == null)
            {
                return;
            }

            // Number of towns to return has changed
            _townCount = model.TownCount;

            // Send the new list
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
                    .Take(_townCount)
                    .Select(s => new TownProsperityItem
                    {
                        Name = s.Name.ToString(),
                        Prosperity = s.Prosperity,
                        Militia = s.Militia,
                        Garrison = s.Parties.Where(p => p.IsGarrison).Sum(p => p.Party.MemberRoster.TotalManCount),
                        FactionName = s.MapFaction.Name.ToString(),
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
