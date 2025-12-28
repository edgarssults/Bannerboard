using Ed.Bannerboard.Models.Widgets;
using Newtonsoft.Json;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// A widget for tracking hero locations.
    /// </summary>
    public class HeroTracker : WidgetBase
    {
        private List<HeroTrackerFilterItem> _trackedHeroes = new List<HeroTrackerFilterItem>();
        private List<Settlement> _oldLocations = new List<Settlement>();

        /// <summary>
        /// A widget for tracking hero locations.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        /// <param name="version">Mod version.</param>
        public HeroTracker(WebSocketServer server, Version version)
            : base(server, version)
        {
        }

        public override void RegisterEvents()
        {
            // Update known locations every hour
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(() =>
            {
                foreach (var session in Server.GetAllSessions())
                {
                    SendUpdate(session);
                }
            }));

            // Update available heroes every day
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(() =>
            {
                foreach (var session in Server.GetAllSessions())
                {
                    Init(session);
                }
            }));
        }

        public override void Init(WebSocketSession session)
        {
            // Send the list of alive heroes for search box
            var model = new HeroTrackerReturnDataModel
            {
                Heroes = Campaign.Current.AliveHeroes
                    .Select(h => new HeroTrackerReturnDataItem
                    {
                        Id = h.StringId,
                        Name = h.Name.ToString()
                    })
					.OrderBy(h => h.Name)
					.ToList(),
                Version = Version
            };
            session.Send(model.ToJsonArraySegment(DefaultVersionConverter));
        }

        public override bool CanHandleMessage(string message)
        {
            // Note: Version is not checked yet
            return Regex.IsMatch(message, $"\"Type\":.*\"{nameof(HeroTrackerFilterModel)}\"");
        }

        public override void HandleMessage(WebSocketSession session, string message)
        {
            var model = JsonConvert.DeserializeObject<HeroTrackerFilterModel>(message, DefaultVersionConverter);
            if (model == null)
            {
                return;
            }

            // List of heroes to track has changed
            _trackedHeroes = model.TrackedHeroes ?? new List<HeroTrackerFilterItem>();

            // Send the new list
            SendUpdate(session);
        }

        private void SendUpdate(WebSocketSession session)
        {
            try
            {
                // Stop tracking old locations
                foreach (var l in _oldLocations)
                {
                    Campaign.Current.VisualTrackerManager.RemoveTrackedObject(l);
                }

                // Start tracking new locations
                // Previously alive, but now dead heroes will still be tracked
                var trackableHeroes = Campaign.Current.AliveHeroes
                    .Union(Campaign.Current.DeadOrDisabledHeroes)
                    .Where(h => _trackedHeroes.Any(t => t.Id == h.StringId))
                    .ToList();
                var newLocations = trackableHeroes
                    .Where(h => _trackedHeroes.First(t => t.Id == h.StringId).IsShownOnMap && h.LastKnownClosestSettlement != null)
                    .Select(h => h.LastKnownClosestSettlement)
                    .ToList();
                foreach (var l in newLocations)
                {
                    Campaign.Current.VisualTrackerManager.RegisterObject(l);
                }

                // Store for next update
                _oldLocations = newLocations.ToList();

                var model = new HeroTrackerModel
                {
                    Heroes = trackableHeroes
                        .Select(h => new HeroTrackerItem
                        {
                            Id = h.StringId,
                            Name = h.Name.ToString(),
                            Location = h.LastKnownClosestSettlement?.Name.ToString() ?? "-",
                            IsDead = h.IsDead,
                            IsDisabled = h.IsDisabled,
                            IsShownOnMap = _trackedHeroes.First(t => t.Id == h.StringId).IsShownOnMap
                        })
                        .ToList(),
                    Version = Version
                };

                session.Send(model.ToJsonArraySegment(DefaultVersionConverter));
            }
            catch
            {
                // Ignore
            }
        }
    }
}
