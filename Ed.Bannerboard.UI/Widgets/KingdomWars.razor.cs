using Blazored.LocalStorage;
using Ed.Bannerboard.Models.Widgets;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class KingdomWars
    {
        private const string VisibleKingdomsKey = "wars-widget-visible-kingdoms";
        private const string ShowMinorFactionsKey = "wars-widget-show-minor-factions";
		private static readonly Version _minimumSupportedVersion = new("0.3.0");
        private KingdomWarsModel? _warsModel;
        private bool _showMinorFactions;
        private List<string>? _visibleKingdoms;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(KingdomWarsModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override Task Update(string model)
        {
            var newModel = JsonConvert.DeserializeObject<KingdomWarsModel>(model, DefaultVersionConverter);
            if (newModel == null)
            {
                return Task.CompletedTask;
            }

			if (!HasModelChanged(_warsModel, newModel))
			{
				return Task.CompletedTask;
			}

			_warsModel = newModel;

			if (_visibleKingdoms == null)
            {
                _visibleKingdoms = _warsModel.Kingdoms.Select(k => k.Name).ToList();
            }

			StateHasChanged();
            return Task.CompletedTask;
		}

		public override async Task ResetAsync()
		{
			_visibleKingdoms = null;
			await LocalStorage!.RemoveItemAsync(VisibleKingdomsKey);
			_showMinorFactions = false;
			await LocalStorage!.RemoveItemAsync(ShowMinorFactionsKey);
			_warsModel = null;

			StateHasChanged();
		}

		protected override async Task OnInitializedAsync()
        {
            _showMinorFactions = await LocalStorage!.GetItemAsync<bool>(ShowMinorFactionsKey);
            _visibleKingdoms = await LocalStorage!.GetItemAsync<List<string>>(VisibleKingdomsKey);
            await base.OnInitializedAsync();
        }

		private bool HasModelChanged(KingdomWarsModel? oldModel, KingdomWarsModel newModel)
		{
			if (oldModel == null)
			{
				return true;
			}

			if (oldModel.Kingdoms.Count != newModel.Kingdoms.Count)
			{
				return true;
			}

			for (int i = 0; i < oldModel.Kingdoms.Count; i++)
			{
				var oldKingdom = oldModel.Kingdoms[i];
				var newKingdom = newModel.Kingdoms[i];

				if (oldKingdom.Name != newKingdom.Name
					|| oldKingdom.PrimaryColor != newKingdom.PrimaryColor
					|| oldKingdom.SecondaryColor != newKingdom.SecondaryColor)
				{
					return true;
				}

				if (oldKingdom.Wars.Count != newKingdom.Wars.Count)
				{
					return true;
				}

				for (int j = 0; j < oldKingdom.Wars.Count; j++)
				{
					var oldWar = oldKingdom.Wars[j];
					var newWar = newKingdom.Wars[j];

					if (oldWar.Name != newWar.Name)
					{
						return true;
					}
				}
			}

			return false;
		}

		private async Task MinorFactionFilterClickedAsync(ChangeEventArgs e)
        {
            _showMinorFactions = (bool)(e.Value ?? true);
            await LocalStorage!.SetItemAsync(ShowMinorFactionsKey, _showMinorFactions);
        }

        private async Task KingdomFilterClickedAsync(KingdomWarsItem kingdom)
        {
            if (_visibleKingdoms == null)
            {
                return;
            }

            if (_visibleKingdoms.Contains(kingdom.Name))
            {
                _visibleKingdoms.Remove(kingdom.Name);
            }
            else
            {
                _visibleKingdoms.Add(kingdom.Name);
            }

            await LocalStorage!.SetItemAsync(VisibleKingdomsKey, _visibleKingdoms);
        }
    }
}
