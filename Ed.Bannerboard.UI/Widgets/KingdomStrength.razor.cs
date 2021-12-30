using Blazored.LocalStorage;
using Blazorise.Charts;
using Ed.Bannerboard.Models.Widgets;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class KingdomStrength
    {
        private readonly Version _minimumSupportedVersion = new("0.3.0");
        private BarChart<float> barChart;
        private KingdomStrengthModel strengthModel;
        private List<string> visibleKingdoms;

        [Inject]
        private ILocalStorageService LocalStorage { get; set; }

        public override bool CanUpdate(object model, Version version)
        {
            return IsCompatible(version, _minimumSupportedVersion) && model is KingdomStrengthModel;
        }

        public override async Task Update(object model)
        {
            strengthModel = model as KingdomStrengthModel;
            if (visibleKingdoms == null)
            {
                visibleKingdoms = strengthModel.Kingdoms.Select(k => k.Name).ToList();
            }

            StateHasChanged();
            await HandleRedraw(strengthModel);
        }

        protected override async Task OnInitializedAsync()
        {
            visibleKingdoms = await LocalStorage.GetItemAsync<List<string>>("strength-widget-visible-kingdoms");
            await base.OnInitializedAsync();
        }

        private async Task HandleRedraw(KingdomStrengthModel model)
        {
            await barChart.Clear();
            var filteredKingdoms = model.Kingdoms.Where(k => visibleKingdoms.Contains(k.Name)).ToList();
            await barChart.AddLabelsDatasetsAndUpdate(GetLabels(filteredKingdoms), GetDataset(filteredKingdoms));
        }

        private async Task KingdomFilterClickedAsync(KingdomStrengthItem kingdom)
        {
            if (visibleKingdoms.Contains(kingdom.Name))
            {
                visibleKingdoms.Remove(kingdom.Name);
            }
            else
            {
                visibleKingdoms.Add(kingdom.Name);
            }

            await LocalStorage.SetItemAsync("strength-widget-visible-kingdoms", visibleKingdoms);
            await HandleRedraw(strengthModel);
        }

        private List<string> GetLabels(List<KingdomStrengthItem> kingdoms) =>
            kingdoms.Select(m => m.Name).ToList();

        private BarChartDataset<float> GetDataset(List<KingdomStrengthItem> kingdoms) =>
            new()
            {
                Data = kingdoms.Select(m => m.Strength).ToList(),
                BackgroundColor = kingdoms.Select(m => m.PrimaryColor).ToList(),
                BorderColor = kingdoms.Select(m => m.SecondaryColor).ToList(),
            };
    }
}
