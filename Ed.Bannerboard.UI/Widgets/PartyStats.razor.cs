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
        private PartyStatsModel? partyModel;
        private bool showUnits = true;
        private bool showFood = true;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(PartyStatsModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override Task Update(string model)
        {
            partyModel = JsonConvert.DeserializeObject<PartyStatsModel>(model, new VersionConverter());
            if (partyModel == null)
            {
                return Task.CompletedTask;
            }

            StateHasChanged();
            return Task.CompletedTask;
        }

        protected override async Task OnInitializedAsync()
        {
            var storedShowUnits = await LocalStorage!.GetItemAsync<bool?>(ShowUnitsKey);
            if (storedShowUnits != null)
            {
                showUnits = storedShowUnits.Value;
            }

            var storedShowFood = await LocalStorage!.GetItemAsync<bool?>(ShowFoodKey);
            if (storedShowFood != null)
            {
                showFood = storedShowFood.Value;
            }

            await base.OnInitializedAsync();
        }

        private async Task ShowUnitsChanged(bool newShowUnits)
        {
            showUnits = newShowUnits;
            await LocalStorage!.SetItemAsync(ShowUnitsKey, showUnits);
        }

        private async Task ShowFoodChanged(bool newShowFood)
        {
            showFood = newShowFood;
            await LocalStorage!.SetItemAsync(ShowFoodKey, showFood);
        }

        private static string GetMemberIcon(MemberStatsItem member)
        {
            return member switch
            {
                { IsInfantry: true } => "infantry",
                { IsArcher: true } => "archer",
                { IsCavalry: true} => "cavalry",
                { IsMountedArcher: true } => "mounted-archer",
                _ => "infantry"
            };
        }
    }
}
