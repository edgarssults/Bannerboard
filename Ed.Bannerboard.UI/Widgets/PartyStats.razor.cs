using Blazored.LocalStorage;
using Ed.Bannerboard.Models.Widgets;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class PartyStats
    {
        private const string ShowUnitsKey = "party-widget-showunits";
        private const string ShowFoodKey = "party-widget-showfood";
        private readonly Version _minimumSupportedVersion = new("0.4.2");
        private PartyStatsModel? _partyModel;
        private bool _showUnits = true;
        private bool _showFood = true;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(PartyStatsModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override Task Update(string model)
        {
            _partyModel = JsonConvert.DeserializeObject<PartyStatsModel>(model, new VersionConverter());
            if (_partyModel == null)
            {
                return Task.CompletedTask;
            }

			// TODO: Check for changes before redrawing
			StateHasChanged();
            return Task.CompletedTask;
		}

		public override async Task ResetAsync()
		{
			_showUnits = true;
			await LocalStorage!.RemoveItemAsync(ShowUnitsKey);
			_showFood = true;
			await LocalStorage!.RemoveItemAsync(ShowFoodKey);
			_partyModel = null;

			StateHasChanged();
		}

		protected override async Task OnInitializedAsync()
        {
            var storedShowUnits = await LocalStorage!.GetItemAsync<bool?>(ShowUnitsKey);
            if (storedShowUnits != null)
            {
                _showUnits = storedShowUnits.Value;
            }

            var storedShowFood = await LocalStorage!.GetItemAsync<bool?>(ShowFoodKey);
            if (storedShowFood != null)
            {
                _showFood = storedShowFood.Value;
            }

            await base.OnInitializedAsync();
        }

        private async Task ShowUnitsChanged(bool newShowUnits)
        {
            _showUnits = newShowUnits;
            await LocalStorage!.SetItemAsync(ShowUnitsKey, _showUnits);
        }

        private async Task ShowFoodChanged(bool newShowFood)
        {
            _showFood = newShowFood;
            await LocalStorage!.SetItemAsync(ShowFoodKey, _showFood);
        }

        private static string GetMemberIcon(MemberStatsItem member)
        {
            return member switch
            {
                { IsInfantry: true } => "infantry",
                { IsArcher: true } => "archer",
                { IsCavalry: true} => "cavalry",
                { IsMountedArcher: true } => "mounted-archer",
				{ IsPrisoner: true } => "prisoner",
                _ => "infantry"
            };
        }
    }
}
