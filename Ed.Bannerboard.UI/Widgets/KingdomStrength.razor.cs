using Blazored.LocalStorage;
using Blazorise.Charts;
using Ed.Bannerboard.Models.Widgets;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class KingdomStrength
    {
        private const string VisibleKingdomsKey = "strength-widget-visible-kingdoms";
        private readonly Version _minimumSupportedVersion = new("0.3.0");
        private BarChart<float>? barChart;
        private KingdomStrengthModel? strengthModel;
        private List<string>? visibleKingdoms;
        private bool isFirstDraw = true;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(KingdomStrengthModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override async Task Update(string model)
        {
            strengthModel = JsonConvert.DeserializeObject<KingdomStrengthModel>(model, new VersionConverter());
            if (strengthModel == null)
            {
                return;
            }

            if (visibleKingdoms == null)
            {
                visibleKingdoms = strengthModel.Kingdoms.Select(k => k.Name).ToList();
            }

            StateHasChanged();

            // Have to delay the first draw for a bit to let JS initialize
            if (isFirstDraw)
            {
                await Task.Delay(200);
                isFirstDraw = false;
            }

            await HandleRedraw(strengthModel);
        }

        protected override async Task OnInitializedAsync()
        {
            visibleKingdoms = await LocalStorage!.GetItemAsync<List<string>>(VisibleKingdomsKey);
            await base.OnInitializedAsync();
        }

        private async Task HandleRedraw(KingdomStrengthModel? model)
        {
            if (visibleKingdoms == null || model == null)
            {
                return;
            }

            await barChart!.Clear();
            var filteredKingdoms = model.Kingdoms.Where(k => visibleKingdoms.Contains(k.Name)).ToList();
            await barChart.AddLabelsDatasetsAndUpdate(GetLabels(filteredKingdoms), GetDataset(filteredKingdoms));
        }

        private async Task KingdomFilterClickedAsync(KingdomStrengthItem kingdom)
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
            await HandleRedraw(strengthModel);
        }

        private static List<string> GetLabels(List<KingdomStrengthItem> kingdoms) =>
            kingdoms.Select(m => m.Name).ToList();

        private static BarChartDataset<float> GetDataset(List<KingdomStrengthItem> kingdoms) =>
            new()
            {
                Data = kingdoms.Select(m => m.Strength).ToList(),
                BackgroundColor = kingdoms.Select(m => m.PrimaryColor).ToList(),
                BorderColor = kingdoms.Select(m => m.SecondaryColor).ToList(),
            };
    }
}
