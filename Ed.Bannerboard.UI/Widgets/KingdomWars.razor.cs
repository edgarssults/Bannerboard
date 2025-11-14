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
        private KingdomWarsModel? warsModel;
        private bool showMinorFactions;
        private List<string>? visibleKingdoms;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(KingdomWarsModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override Task Update(string model)
        {
            warsModel = JsonConvert.DeserializeObject<KingdomWarsModel>(model, new VersionConverter());
            if (warsModel == null)
            {
                return Task.CompletedTask;
            }

            if (visibleKingdoms == null)
            {
                visibleKingdoms = warsModel.Kingdoms.Select(k => k.Name).ToList();
            }

            StateHasChanged();
            return Task.CompletedTask;
		}

		public override async Task ResetAsync()
		{
			visibleKingdoms = null;
			await LocalStorage!.RemoveItemAsync(VisibleKingdomsKey);
			showMinorFactions = false;
			await LocalStorage!.RemoveItemAsync(ShowMinorFactionsKey);
			warsModel = null;

			StateHasChanged();
		}

		protected override async Task OnInitializedAsync()
        {
            showMinorFactions = await LocalStorage!.GetItemAsync<bool>(ShowMinorFactionsKey);
            visibleKingdoms = await LocalStorage!.GetItemAsync<List<string>>(VisibleKingdomsKey);
            await base.OnInitializedAsync();
        }

        private async Task MinorFactionFilterClickedAsync(ChangeEventArgs e)
        {
            showMinorFactions = (bool)(e.Value ?? true);
            await LocalStorage!.SetItemAsync(ShowMinorFactionsKey, showMinorFactions);
        }

        private async Task KingdomFilterClickedAsync(KingdomWarsItem kingdom)
        {
            if (visibleKingdoms == null)
            {
                return;
            }

            if (visibleKingdoms.Contains(kingdom.Name))
            {
                visibleKingdoms.Remove(kingdom.Name);
            }
            else
            {
                visibleKingdoms.Add(kingdom.Name);
            }

            await LocalStorage!.SetItemAsync(VisibleKingdomsKey, visibleKingdoms);
        }
    }
}
