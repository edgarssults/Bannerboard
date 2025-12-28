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
		private static readonly Version _minimumSupportedVersion = new("0.3.3");
        private TownProsperityModel? _prosperityModel;
        private int _townCount = 10;
        private BarChart<float>? _barChart;
        private ProsperityView _view = ProsperityView.Table;
        private bool _isFirstDraw = true;

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
            var newModel = JsonConvert.DeserializeObject<TownProsperityModel>(model, DefaultVersionConverter);
            if (newModel == null)
            {
                return;
            }

			if (!HasModelChanged(_prosperityModel, newModel))
			{
				return;
			}

			_prosperityModel = newModel;

			StateHasChanged();

            if (_view == ProsperityView.Chart)
            {
                await HandleRedraw(_prosperityModel, _isFirstDraw);
				_isFirstDraw = false;
			}
        }

        public override void SendInitialMessage()
        {
            // Widget is now initialized and message event is subscribed to, request data from the server
            SendFilterMessage();
		}

		public override async Task ResetAsync()
		{
			_townCount = 10;
			await LocalStorage!.RemoveItemAsync(TownCountKey);
			_view = ProsperityView.Table;
			await LocalStorage!.RemoveItemAsync(ViewKey);
			_prosperityModel = null;

			StateHasChanged();
			SendFilterMessage();
		}

		protected override async Task OnInitializedAsync()
        {
            var storedTownCount = await LocalStorage!.GetItemAsync<int?>(TownCountKey);
            if (storedTownCount != null)
            {
                _townCount = storedTownCount.Value;
            }

            var storedView = await LocalStorage!.GetItemAsync<ProsperityView?>(ViewKey);
            if (storedView != null)
            {
                _view = storedView.Value;
            }

            await base.OnInitializedAsync();
        }

		private bool HasModelChanged(TownProsperityModel? oldModel, TownProsperityModel newModel)
		{
			if (oldModel == null)
			{
				return true;
			}

			if (oldModel.Towns.Count != newModel.Towns.Count)
			{
				return true;
			}

			for (int i = 0; i < oldModel.Towns.Count; i++)
			{
				var oldTown = oldModel.Towns[i];
				var newTown = newModel.Towns[i];

				if (oldTown.Name != newTown.Name
					|| oldTown.Prosperity != newTown.Prosperity
					|| oldTown.Garrison != newTown.Garrison
					|| oldTown.Militia != newTown.Militia
					|| oldTown.FactionName != newTown.FactionName
					|| oldTown.PrimaryColor != newTown.PrimaryColor
					|| oldTown.SecondaryColor != newTown.SecondaryColor)
				{
					return true;
				}
			}

			return false;
		}

		private async Task ProsperityFilterClickedAsync(int value)
        {
            _townCount = value;
            await LocalStorage!.SetItemAsync(TownCountKey, _townCount);
            SendFilterMessage();
        }

        private async Task ProsperityViewChangedAsync(ProsperityView value)
        {
            _view = value;
            await LocalStorage!.SetItemAsync(ViewKey, _view);

            if (_view == ProsperityView.Chart)
            {
                await HandleRedraw(_prosperityModel, true);
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

			// Blazorise requires a license for more than 10 data points
			var topTenTowns = model.Towns.Take(10).ToList();

			await _barChart!.Clear();
            await _barChart.AddLabelsDatasetsAndUpdate(GetLabels(topTenTowns), GetDataset(topTenTowns));
        }

        private void SendFilterMessage()
        {
            var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
            var model = new TownProsperityFilterModel
            {
                TownCount = _townCount,
                Version = new Version(settings!.Version)
            };
            OnMessageSent(JsonConvert.SerializeObject(model));
        }

        private static List<string> GetLabels(List<TownProsperityItem> towns) =>
            towns.Select(m => m.Name).ToList();

        private static BarChartDataset<float> GetDataset(List<TownProsperityItem> towns) =>
            new()
            {
				Data = towns.Select(m => (float)Math.Round(m.Prosperity, 0)).ToList(),
				BackgroundColor = towns.Select(m => m.PrimaryColor).ToList(),
                BorderColor = towns.Select(m => m.SecondaryColor).ToList()
            };
    }
}
