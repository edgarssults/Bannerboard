using Ed.Bannerboard.Models.Widgets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SuperSocket.Common;
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
            // Not sending anything because the client will request which heroes to track
        }

        public override bool CanHandleMessage(string message)
        {
            // Note: Version is not checked yet
            return Regex.IsMatch(message, $"\"Type\":.*\"{nameof(HeroTrackerFilterModel)}\"");
        }

        public override void HandleMessage(WebSocketSession session, string message)
        {
            var model = JsonConvert.DeserializeObject<HeroTrackerFilterModel>(message, new VersionConverter());
            if (model == null)
            {
                return;
            }

            // List of heroes to track has changed
            _trackedHeroes = model.TrackedHeroes;

            // TODO: Search instead
            if (!_trackedHeroes.Any())
            {
                // Add faction leaders to the list except the player
                _trackedHeroes.AddRange(Campaign.Current.Kingdoms
                    .Where(k => !k.Leader.IsHumanPlayerCharacter)
                    .Select(k => new HeroTrackerFilterItem
                    {
                        Id = k.Leader.StringId,
                        IsShownOnMap = true
                    }));
            }

            // Send the new list
            SendUpdate(session);
        }

        private void SendUpdate(WebSocketSession session)
        {
            // Stop tracking old locations
            foreach (var l in _oldLocations)
            {
                Campaign.Current.VisualTrackerManager.RemoveTrackedObject(l);
            }

            // Start tracking new locations
            var trackableHeroes = Campaign.Current.AliveHeroes
                .Union(Campaign.Current.DeadOrDisabledHeroes)
                .Where(h => _trackedHeroes.Any(t => t.Id == h.StringId))
                .ToList();
            var newLocations = trackableHeroes
                .Where(h => _trackedHeroes.First(t => t.Id == h.StringId).IsShownOnMap)
                .Select(h => h.LastSeenPlace)
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
                        Location = h.LastSeenPlace.Name.ToString(),
                        DaysAgo = h.LastSeenTime.ElapsedDaysUntilNow,
                        IsDead = h.IsDead,
                        IsDisabled = h.IsDisabled,
                        IsShownOnMap = _trackedHeroes.First(t => t.Id == h.StringId).IsShownOnMap
                    })
                    .ToList(),
                Version = Version,
            };

            session.Send(model.ToJsonArraySegment());
        }
    }
}
