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
		private static readonly Version _minimumSupportedVersion = new("0.4.2");
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
            var newModel = JsonConvert.DeserializeObject<PartyStatsModel>(model, DefaultVersionConverter);
            if (newModel == null)
            {
                return Task.CompletedTask;
            }

			if (!HasModelChanged(_partyModel, newModel))
			{
				return Task.CompletedTask;
			}

			_partyModel = newModel;

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

		private bool HasModelChanged(PartyStatsModel? oldModel, PartyStatsModel newModel)
		{
			if (oldModel == null)
			{
				return true;
			}

			if (oldModel.Food.Items.Count != newModel.Food.Items.Count)
			{
				return true;
			}

			for (int i = 0; i < oldModel.Food.Items.Count; i++)
			{
				var oldFood = oldModel.Food.Items[i];
				var newFood = newModel.Food.Items[i];

				if (oldFood.Name != newFood.Name
					|| oldFood.Count != newFood.Count)
				{
					return true;
				}
			}

			if (oldModel.Members.MaxCount != newModel.Members.MaxCount
				|| oldModel.Members.TotalRegulars != newModel.Members.TotalRegulars
				|| oldModel.Members.TotalHeroes != newModel.Members.TotalHeroes)
			{
				return true;
			}

			if (oldModel.Members.Items.Count != newModel.Members.Items.Count)
			{
				return true;
			}

			for (int i = 0; i < oldModel.Members.Items.Count; i++)
			{
				var oldMember = oldModel.Members.Items[i];
				var newMember = newModel.Members.Items[i];

				if (oldMember.Count != newMember.Count
					|| oldMember.WoundedCount != newMember.WoundedCount
					|| oldMember.IsInfantry != newMember.IsInfantry
					|| oldMember.IsArcher != newMember.IsArcher
					|| oldMember.IsCavalry != newMember.IsCavalry
					|| oldMember.IsMountedArcher != newMember.IsMountedArcher
					|| oldMember.IsPrisoner != newMember.IsPrisoner)
				{
					return true;
				}
			}

			return false;
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
