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
    public partial class KingdomLords
    {
        private readonly Version _minimumSupportedVersion = new("0.3.0");
        private BarChart<int> barChart;
        private KingdomLordsModel lordsModel;
        private List<string> visibleKingdoms;

        [Inject]
        private ILocalStorageService LocalStorage { get; set; }

        public override bool CanUpdate(object model, Version version)
        {
            return IsCompatible(version, _minimumSupportedVersion) && model is KingdomLordsModel;
        }

        public override async Task Update(object model)
        {
            lordsModel = model as KingdomLordsModel;
            if (visibleKingdoms == null)
            {
                visibleKingdoms = lordsModel.Kingdoms.Select(k => k.Name).ToList();
            }

            StateHasChanged();
            await HandleRedraw(lordsModel);
        }

        protected override async Task OnInitializedAsync()
        {
            visibleKingdoms = await LocalStorage.GetItemAsync<List<string>>("lords-widget-visible-kingdoms");
            await base.OnInitializedAsync();
        }

        private async Task HandleRedraw(KingdomLordsModel model)
        {
            await barChart.Clear();
            var filteredKingdoms = model.Kingdoms.Where(k => visibleKingdoms.Contains(k.Name)).ToList();
            await barChart.AddLabelsDatasetsAndUpdate(GetLabels(filteredKingdoms), GetDataset(filteredKingdoms));
        }

        private async Task KingdomFilterClickedAsync(KingdomLordsItem kingdom)
        {
            if (visibleKingdoms.Contains(kingdom.Name))
            {
                visibleKingdoms.Remove(kingdom.Name);
            }
            else
            {
                visibleKingdoms.Add(kingdom.Name);
            }

            await LocalStorage.SetItemAsync("lords-widget-visible-kingdoms", visibleKingdoms);
            await HandleRedraw(lordsModel);
        }

        private List<string> GetLabels(List<KingdomLordsItem> kingdoms) =>
            kingdoms.Select(m => m.Name).ToList();

        private BarChartDataset<int> GetDataset(List<KingdomLordsItem> kingdoms) =>
            new()
            {
                Data = kingdoms.Select(m => m.Lords).ToList(),
                BackgroundColor = kingdoms.Select(m => m.PrimaryColor).ToList(),
                BorderColor = kingdoms.Select(m => m.SecondaryColor).ToList(),
            };
    }
}
