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
		private static readonly Version _minimumSupportedVersion = new("0.3.0");
        private BarChart<float>? _barChart;
        private KingdomStrengthModel? _strengthModel;
        private List<string>? _visibleKingdoms;
        private bool _isFirstDraw = true;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(KingdomStrengthModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override async Task Update(string model)
        {
			var newModel = JsonConvert.DeserializeObject<KingdomStrengthModel>(model, DefaultVersionConverter);
            if (newModel == null)
            {
                return;
            }

			if (!HasModelChanged(_strengthModel, newModel))
			{
				return;
			}

			_strengthModel = newModel;

			if (_visibleKingdoms == null)
            {
                _visibleKingdoms = _strengthModel.Kingdoms.Select(k => k.Name).ToList();
            }

			StateHasChanged();

            // Have to delay the first draw for a bit to let JS initialize
            if (_isFirstDraw)
            {
                await Task.Delay(200);
                _isFirstDraw = false;
            }

            await HandleRedraw(_strengthModel);
		}

		public override async Task ResetAsync()
		{
			_visibleKingdoms = null;
			await LocalStorage!.RemoveItemAsync(VisibleKingdomsKey);
			_strengthModel = null;

			StateHasChanged();
		}

		protected override async Task OnInitializedAsync()
        {
            _visibleKingdoms = await LocalStorage!.GetItemAsync<List<string>>(VisibleKingdomsKey);
            await base.OnInitializedAsync();
        }

		private bool HasModelChanged(KingdomStrengthModel? oldModel, KingdomStrengthModel newModel)
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
					|| oldKingdom.Strength != newKingdom.Strength
					|| oldKingdom.PrimaryColor != newKingdom.PrimaryColor
					|| oldKingdom.SecondaryColor != newKingdom.SecondaryColor)
				{
					return true;
				}
			}

			return false;
		}

		private async Task HandleRedraw(KingdomStrengthModel? model)
        {
            if (_visibleKingdoms == null || model == null)
            {
                return;
            }

            await _barChart!.Clear();
            var filteredKingdoms = model.Kingdoms.Where(k => _visibleKingdoms.Contains(k.Name)).ToList();
            await _barChart.AddLabelsDatasetsAndUpdate(GetLabels(filteredKingdoms), GetDataset(filteredKingdoms));
        }

        private async Task KingdomFilterClickedAsync(KingdomStrengthItem kingdom)
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
            await HandleRedraw(_strengthModel);
        }

        private static List<string> GetLabels(List<KingdomStrengthItem> kingdoms) =>
            kingdoms.Select(m => m.Name).ToList();

        private static BarChartDataset<float> GetDataset(List<KingdomStrengthItem> kingdoms) =>
            new()
            {
                Data = kingdoms.Select(m => (float)Math.Round(m.Strength, 0)).ToList(),
                BackgroundColor = kingdoms.Select(m => m.PrimaryColor).ToList(),
                BorderColor = kingdoms.Select(m => m.SecondaryColor).ToList(),
            };
    }
}
