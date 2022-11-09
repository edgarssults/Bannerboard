using Blazored.LocalStorage;
using Blazorise.Components;
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
        private Autocomplete<HeroTrackerReturnDataItem, string>? heroSearch;
        private List<HeroTrackerReturnDataItem> allHeroes = new();
        private List<HeroTrackerFilterItem> trackedHeroes = new();

        private string? SelectedText { get; set; }

        [Inject]
        private IConfiguration? Configuration { get; set; }

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return (Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerModel)}\"")
                || Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerReturnDataModel)}\""))
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override async Task Update(string model)
        {
            if (Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerModel)}\""))
            {
                // Received tracking information
                heroModel = JsonConvert.DeserializeObject<HeroTrackerModel>(model, new VersionConverter());
                if (heroModel == null)
                {
                    return;
                }

                StateHasChanged();
            }
            else if (Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerReturnDataModel)}\""))
            {
                // Received list of trackable heroes
                var data = JsonConvert.DeserializeObject<HeroTrackerReturnDataModel>(model, new VersionConverter());
                if (data == null)
                {
                    return;
                }

                allHeroes = data.Heroes;
            }
        }

        public override void SendInitialMessage()
        {
            // Widget is now initialized and message event is subscribed to, request data from the server
            // TODO: Load previous filters from storage
            SendFilterMessage();
        }

        private void SendFilterMessage()
        {
            var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
            var model = new HeroTrackerFilterModel
            {
                TrackedHeroes = trackedHeroes,
                Version = new Version(settings.Version)
            };
            OnMessageSent(JsonConvert.SerializeObject(model));
        }

        private void TrackingStatusChanged(HeroTrackerItem hero)
        {
            // Updated view model
            hero.IsShownOnMap = !hero.IsShownOnMap;

            // Update tracking model
            trackedHeroes.First(h => h.Id == hero.Id).IsShownOnMap = hero.IsShownOnMap;

            // Request new data
            SendFilterMessage();
        }

        private void SelectedHeroChanged(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Should not happen
                return;
            }

            if (trackedHeroes.Any(h => h.Id == id))
            {
                // Hero is already being tracked
                return;
            }

            // Update tracking model
            trackedHeroes.Add(new HeroTrackerFilterItem
            {
                Id = id,
                IsShownOnMap = true
            });

            // Clear the text
            SelectedText = null;

            // Request new data
            SendFilterMessage();
        }

        private void RemoveHero(HeroTrackerItem hero)
        {
            // Updated view model
            heroModel?.Heroes.Remove(hero);

            // Update tracking model
            trackedHeroes.RemoveAll(h => h.Id == hero.Id);

            // Request new data
            SendFilterMessage();
        }
    }
}
