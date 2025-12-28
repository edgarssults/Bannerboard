using Blazored.LocalStorage;
using Blazorise.Charts;
using Ed.Bannerboard.Models.Widgets;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class KingdomLords
    {
        private const string VisibleKingdomsKey = "lords-widget-visible-kingdoms";
        private static readonly Version _minimumSupportedVersion = new("0.3.0");
        private BarChart<int>? _barChart;
        private KingdomLordsModel? _lordsModel;
        private List<string>? _visibleKingdoms;
        private bool _isFirstDraw = true;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(KingdomLordsModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override async Task Update(string model)
        {
            var newModel = JsonConvert.DeserializeObject<KingdomLordsModel>(model, DefaultVersionConverter);
            if (newModel == null)
            {
                return;
            }

			if (!HasModelChanged(_lordsModel, newModel))
			{
				return;
			}

			_lordsModel = newModel;

			if (_visibleKingdoms == null)
			{
				_visibleKingdoms = _lordsModel.Kingdoms.Select(k => k.Name).ToList();
			}

			StateHasChanged();

            // Have to delay the first draw for a bit to let JS initialize
            if (_isFirstDraw)
            {
                await Task.Delay(200);
                _isFirstDraw = false;
            }

            await HandleRedraw(_lordsModel);
		}

		public override async Task ResetAsync()
		{
			_visibleKingdoms = null;
			await LocalStorage!.RemoveItemAsync(VisibleKingdomsKey);
			_lordsModel = null;

			StateHasChanged();
		}

		protected override async Task OnInitializedAsync()
        {
            _visibleKingdoms = await LocalStorage!.GetItemAsync<List<string>>(VisibleKingdomsKey);
            await base.OnInitializedAsync();
        }

		private bool HasModelChanged(KingdomLordsModel? oldModel, KingdomLordsModel newModel)
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
					|| oldKingdom.Lords != newKingdom.Lords
					|| oldKingdom.PrimaryColor != newKingdom.PrimaryColor
					|| oldKingdom.SecondaryColor != newKingdom.SecondaryColor)
				{
					return true;
				}
			}

			return false;
		}

        private async Task HandleRedraw(KingdomLordsModel? model)
        {
            if (_visibleKingdoms == null || model == null)
            {
                return;
            }

            await _barChart!.Clear();
            var filteredKingdoms = model.Kingdoms.Where(k => _visibleKingdoms.Contains(k.Name)).ToList();
            await _barChart.AddLabelsDatasetsAndUpdate(GetLabels(filteredKingdoms), GetDataset(filteredKingdoms));
        }

        private async Task KingdomFilterClickedAsync(KingdomLordsItem kingdom)
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
            await HandleRedraw(_lordsModel);
        }

        private static List<string> GetLabels(List<KingdomLordsItem> kingdoms) =>
            kingdoms.Select(m => m.Name).ToList();

        private static BarChartDataset<int> GetDataset(List<KingdomLordsItem> kingdoms) =>
            new()
            {
                Data = kingdoms.Select(m => m.Lords).ToList(),
                BackgroundColor = kingdoms.Select(m => m.PrimaryColor).ToList(),
                BorderColor = kingdoms.Select(m => m.SecondaryColor).ToList(),
            };
    }
}
