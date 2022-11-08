using Ed.Bannerboard.Models.Widgets;
using Ed.Bannerboard.UI.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class HeroTracker
    {
        private readonly Version _minimumSupportedVersion = new("0.4.0");
        private HeroTrackerModel? heroModel;

        [Inject]
        private IConfiguration? Configuration { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override async Task Update(string model)
        {
            heroModel = JsonConvert.DeserializeObject<HeroTrackerModel>(model, new VersionConverter());
            if (heroModel == null)
            {
                return;
            }

            StateHasChanged();
        }

        public override void SendInitialMessage()
        {
            // Widget is now initialized and message event is subscribed to, request data from the server
            SendFilterMessage();
        }

        private void SendFilterMessage()
        {
            var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
            var model = new HeroTrackerFilterModel
            {
                // TODO: Search
                // TODO: Filter from storage
                TrackedHeroes = heroModel != null
                    ? heroModel.Heroes
                        .Select(h => new HeroTrackerFilterItem
                        {
                            Id = h.Id,
                            IsShownOnMap = h.IsShownOnMap,
                        })
                        .ToList()
                    : new List<HeroTrackerFilterItem>(),
                Version = new Version(settings.Version)
            };
            OnMessageSent(JsonConvert.SerializeObject(model));
        }

        private void TrackingStatusChanged(HeroTrackerItem hero)
        {
            hero.IsShownOnMap = !hero.IsShownOnMap;
            SendFilterMessage();
        }

        private void RemoveHero(HeroTrackerItem hero)
        {
            if (heroModel == null)
            {
                return;
            }

            heroModel.Heroes.Remove(hero);
            SendFilterMessage();
        }
    }
}
