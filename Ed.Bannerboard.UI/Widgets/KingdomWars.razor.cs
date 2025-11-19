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
        private readonly Version _minimumSupportedVersion = new("0.3.0");
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
            _warsModel = JsonConvert.DeserializeObject<KingdomWarsModel>(model, new VersionConverter());
            if (_warsModel == null)
            {
                return Task.CompletedTask;
            }

            if (_visibleKingdoms == null)
            {
                _visibleKingdoms = _warsModel.Kingdoms.Select(k => k.Name).ToList();
            }

			// TODO: Check for changes before redrawing
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
