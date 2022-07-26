using Blazored.LocalStorage;
using Blazorise.Charts;
using Ed.Bannerboard.Models.Widgets;
using Ed.Bannerboard.UI.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class TownProsperity
    {
        private const string TownCountKey = "prosperity-widget-town-count";
        private const string ViewKey = "prosperity-widget-view";
        private readonly Version _minimumSupportedVersion = new("0.3.3");
        private TownProsperityModel? prosperityModel;
        private int townCount = 10;
        private BarChart<float>? barChart;
        private ProsperityView view = ProsperityView.Table;
        private bool isFirstDraw = true;

        private enum ProsperityView
        {
            Table,
            Chart
        }

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        [Inject]
        private IConfiguration? Configuration { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(TownProsperityModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override async Task Update(string model)
        {
            prosperityModel = JsonConvert.DeserializeObject<TownProsperityModel>(model, new VersionConverter());
            if (prosperityModel == null)
            {
                return;
            }

            StateHasChanged();

            if (view == ProsperityView.Chart)
            {
                await HandleRedraw(prosperityModel, isFirstDraw);
            }
        }

        public override void SendInitialMessage()
        {
            // Widget is now initialized and message event is subscribed to, request data from the server
            SendFilterMessage();
        }

        protected override async Task OnInitializedAsync()
        {
            var storedTownCount = await LocalStorage!.GetItemAsync<int?>(TownCountKey);
            if (storedTownCount != null)
            {
                townCount = storedTownCount.Value;
            }

            var storedView = await LocalStorage!.GetItemAsync<ProsperityView?>(ViewKey);
            if (storedView != null)
            {
                view = storedView.Value;
            }

            await base.OnInitializedAsync();
        }

        private async Task ProsperityFilterClickedAsync(int count)
        {
            townCount = count;
            await LocalStorage!.SetItemAsync(TownCountKey, townCount);
            SendFilterMessage();
        }

        private async Task ProsperityViewChangedAsync(ProsperityView newView)
        {
            view = newView;
            await LocalStorage!.SetItemAsync(ViewKey, view);

            if (view == ProsperityView.Chart)
            {
                await HandleRedraw(prosperityModel, true);
            }
        }

        private async Task HandleRedraw(TownProsperityModel? model, bool withDelay = false)
        {
            if (model == null)
            {
                return;
            }

            // Have to wait, otherwise the data doesn't show
            if (withDelay)
            {
                await Task.Delay(200);
            }

            await barChart!.Clear();
            await barChart.AddLabelsDatasetsAndUpdate(GetLabels(model.Towns), GetDataset(model.Towns));
        }

        private void SendFilterMessage()
        {
            var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
            var model = new TownProsperityFilterModel
            {
                TownCount = townCount,
                Version = new Version(settings.Version)
            };
            OnMessageSent(JsonConvert.SerializeObject(model));
        }

        private static List<string> GetLabels(List<TownProsperityItem> towns) =>
            towns.Select(m => m.Name).ToList();

        private static BarChartDataset<float> GetDataset(List<TownProsperityItem> towns) =>
            new()
            {
                Data = towns.Select(m => m.Prosperity).ToList(),
                BackgroundColor = towns.Select(m => m.PrimaryColor).ToList(),
                BorderColor = towns.Select(m => m.SecondaryColor).ToList()
            };
    }
}
