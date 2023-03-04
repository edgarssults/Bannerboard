﻿using Blazored.LocalStorage;
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
        private const string TrackedHeroesKey = "heroes-widget-tracked-heroes";
        private readonly Version _minimumSupportedVersion = new("0.4.2");
        private HeroTrackerModel? heroModel;
        private Autocomplete<HeroTrackerReturnDataItem, string>? heroSearch;
        private List<HeroTrackerReturnDataItem>? allHeroes;
        private List<HeroTrackerFilterItem>? trackedHeroes;
        private bool isSearchBoxVisible;

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

        public override Task Update(string model)
        {
            if (Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerModel)}\""))
            {
                // Received tracking information
                heroModel = JsonConvert.DeserializeObject<HeroTrackerModel>(model, new VersionConverter());
                if (heroModel == null)
                {
                    return Task.CompletedTask;
                }

                StateHasChanged();
            }
            else if (Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerReturnDataModel)}\""))
            {
                // Received list of trackable heroes
                var data = JsonConvert.DeserializeObject<HeroTrackerReturnDataModel>(model, new VersionConverter());
                if (data == null)
                {
                    return Task.CompletedTask;
                }

                allHeroes = data.Heroes;

                StateHasChanged();
            }

            return Task.CompletedTask;
        }

        protected override async Task OnInitializedAsync()
        {
            trackedHeroes = await LocalStorage!.GetItemAsync<List<HeroTrackerFilterItem>>(TrackedHeroesKey) ?? new List<HeroTrackerFilterItem>();
            await base.OnInitializedAsync();
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
                TrackedHeroes = trackedHeroes,
                Version = new Version(settings.Version)
            };
            OnMessageSent(JsonConvert.SerializeObject(model));
        }

        private async Task TrackingStatusChangedAsync(HeroTrackerItem hero)
        {
            if (trackedHeroes == null)
            {
                return;
            }

            // Updated view model
            hero.IsShownOnMap = !hero.IsShownOnMap;

            // Update tracking model
            trackedHeroes.First(h => h.Id == hero.Id).IsShownOnMap = hero.IsShownOnMap;
            await LocalStorage!.SetItemAsync(TrackedHeroesKey, trackedHeroes);

            // Request new data
            SendFilterMessage();
        }

        private async Task SelectedHeroChangedAsync(string id)
        {
            if (trackedHeroes == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (trackedHeroes.Any(h => h.Id == id))
            {
                // Hero is already being tracked
                SelectedText = null;
                return;
            }

            // Update tracking model
            trackedHeroes.Add(new HeroTrackerFilterItem
            {
                Id = id,
                IsShownOnMap = true
            });
            await LocalStorage!.SetItemAsync(TrackedHeroesKey, trackedHeroes);

            // Clear the text
            SelectedText = null;
            isSearchBoxVisible = false;

            // Request new data
            SendFilterMessage();
        }

        private async Task RemoveHeroAsync(HeroTrackerItem hero)
        {
            if (trackedHeroes == null)
            {
                return;
            }

            // Updated view model
            heroModel?.Heroes.Remove(hero);

            // Update tracking model
            trackedHeroes.RemoveAll(h => h.Id == hero.Id);
            await LocalStorage!.SetItemAsync(TrackedHeroesKey, trackedHeroes);

            // Request new data
            SendFilterMessage();
        }

        private async Task ShowSearchBox()
        {
            isSearchBoxVisible = true;
            await Task.Delay(200); // Wait, otherwise focus doesn't work
            heroSearch?.Focus();
        }
    }
}
