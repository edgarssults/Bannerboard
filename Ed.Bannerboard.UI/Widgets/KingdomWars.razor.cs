using Blazored.LocalStorage;
using Ed.Bannerboard.Models.Widgets;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class KingdomWars
    {
        private readonly Version _minimumSupportedVersion = new("0.3.0");
        private KingdomWarsModel warsModel;
        private bool showMinorFactions;
        private List<string> visibleKingdoms;

        [Inject]
        private ILocalStorageService LocalStorage { get; set; }

        public override bool CanUpdate(object model, Version version)
        {
            return IsCompatible(version, _minimumSupportedVersion) && model is KingdomWarsModel;
        }

        public override Task Update(object model)
        {
            warsModel = model as KingdomWarsModel;
            if (visibleKingdoms == null)
            {
                visibleKingdoms = warsModel.Kingdoms.Select(k => k.Name).ToList();
            }

            StateHasChanged();
            return Task.CompletedTask;
        }

        protected override async Task OnInitializedAsync()
        {
            showMinorFactions = await LocalStorage.GetItemAsync<bool>("wars-widget-show-minor-factions");
            visibleKingdoms = await LocalStorage.GetItemAsync<List<string>>("wars-widget-visible-kingdoms");
            await base.OnInitializedAsync();
        }

        private async Task MinorFactionFilterClickedAsync(ChangeEventArgs e)
        {
            showMinorFactions = (bool)e.Value;
            await LocalStorage.SetItemAsync("wars-widget-show-minor-factions", showMinorFactions);
        }

        private async Task KingdomFilterClickedAsync(KingdomWarsItem kingdom)
        {
            if (visibleKingdoms.Contains(kingdom.Name))
            {
                visibleKingdoms.Remove(kingdom.Name);
            }
            else
            {
                visibleKingdoms.Add(kingdom.Name);
            }

            await LocalStorage.SetItemAsync("wars-widget-visible-kingdoms", visibleKingdoms);
        }
    }
}
