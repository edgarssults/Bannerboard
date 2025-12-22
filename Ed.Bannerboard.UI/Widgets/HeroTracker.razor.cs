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
        private const string TrackedHeroesKey = "heroes-widget-tracked-heroes";
        private readonly Version _minimumSupportedVersion = new("0.4.2");
        private HeroTrackerModel? _heroModel;
        private Autocomplete<HeroTrackerReturnDataItem, string>? _heroSearch;
        private List<HeroTrackerReturnDataItem>? _allHeroes;
        private List<HeroTrackerFilterItem>? _trackedHeroes;
        private bool _isSearchBoxVisible;

        private string? SelectedText { get; set; }

        [Inject]
        private IConfiguration? Configuration { get; set; }

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return
				(
					Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerModel)}\"")
					||
					Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerReturnDataModel)}\"")
				)
                &&
				IsCompatible(version, _minimumSupportedVersion);
        }

        public override Task Update(string model)
        {
            if (Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerModel)}\""))
            {
                // Received tracking information
                var newHeroModel = JsonConvert.DeserializeObject<HeroTrackerModel>(model, DefaultVersionConverter);
                if (newHeroModel == null)
                {
                    return Task.CompletedTask;
				}

				if (_heroModel != null
					&& newHeroModel.Heroes.SequenceEqual(_heroModel.Heroes))
				{
					// Do not update if nothing has changed
					return Task.CompletedTask;
				}

				_heroModel = newHeroModel;

                StateHasChanged();
            }
            else if (Regex.IsMatch(model, $"\"Type\":.*\"{nameof(HeroTrackerReturnDataModel)}\""))
            {
                // Received list of trackable heroes
                var newHeroes = JsonConvert.DeserializeObject<HeroTrackerReturnDataModel>(model, DefaultVersionConverter);
                if (newHeroes == null)
                {
                    return Task.CompletedTask;
                }

				if (_allHeroes != null
					&& newHeroes.Heroes.SequenceEqual(_allHeroes))
				{
					// Do not update if nothing has changed
					return Task.CompletedTask;
				}

				_allHeroes = newHeroes.Heroes;

                StateHasChanged();
            }

            return Task.CompletedTask;
		}

		public override async Task ResetAsync()
		{
			_trackedHeroes = [];
			await LocalStorage!.RemoveItemAsync(TrackedHeroesKey);
			_heroModel = null;

			StateHasChanged();
			SendFilterMessage();
		}

		protected override async Task OnInitializedAsync()
        {
            _trackedHeroes = await LocalStorage!.GetItemAsync<List<HeroTrackerFilterItem>>(TrackedHeroesKey) ?? [];
            await base.OnInitializedAsync();
        }

        public override void SendInitialMessage()
        {
            // Widget is now initialized and message event is subscribed to, request data from the server
            SendFilterMessage();
        }

        private void SendFilterMessage()
        {
            var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>()!;
            var model = new HeroTrackerFilterModel
            {
                TrackedHeroes = _trackedHeroes,
                Version = new Version(settings.Version)
            };
            OnMessageSent(JsonConvert.SerializeObject(model));
        }

        private async Task TrackingStatusChangedAsync(HeroTrackerItem hero)
        {
            if (_trackedHeroes == null)
            {
                return;
            }

            // Updated view model
            hero.IsShownOnMap = !hero.IsShownOnMap;

			// Update tracking model
			var trackedHero = _trackedHeroes.FirstOrDefault(h => h.Id == hero.Id);
			if (trackedHero != null)
			{
				trackedHero.IsShownOnMap = hero.IsShownOnMap;
			}
			await LocalStorage!.SetItemAsync(TrackedHeroesKey, _trackedHeroes);

            // Request new data
            SendFilterMessage();
        }

        private async Task SelectedHeroChangedAsync(string id)
        {
            if (_trackedHeroes == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (_trackedHeroes.Any(h => h.Id == id))
            {
                // Hero is already being tracked
                SelectedText = null;
                return;
            }

            // Update tracking model
            _trackedHeroes.Add(new HeroTrackerFilterItem
            {
                Id = id,
                IsShownOnMap = true
            });
            await LocalStorage!.SetItemAsync(TrackedHeroesKey, _trackedHeroes);

            // Clear the text
            SelectedText = null;
            _isSearchBoxVisible = false;

            // Request new data
            SendFilterMessage();
        }

        private async Task RemoveHeroAsync(HeroTrackerItem hero)
        {
            if (_trackedHeroes == null)
            {
                return;
            }

            // Updated view model
            _heroModel?.Heroes.Remove(hero);

            // Update tracking model
            _trackedHeroes.RemoveAll(h => h.Id == hero.Id);
            await LocalStorage!.SetItemAsync(TrackedHeroesKey, _trackedHeroes);

            // Request new data
            SendFilterMessage();
        }

        private async Task ShowSearchBox()
        {
            _isSearchBoxVisible = true;
            await Task.Delay(200); // Wait, otherwise focus doesn't work
            _heroSearch?.Focus();
        }
    }
}
