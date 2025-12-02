using Blazored.LocalStorage;
using Ed.Bannerboard.Models.Widgets;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
	public partial class TradePrices
	{
		private const string SelectedTownKey = "trade-prices-selected-town";
		private readonly Version _minimumSupportedVersion = new("0.3.0");
		private TradePricesModel? _tradePricesModel;
		private string? _selectedTown = null;
		private List<string>? _availableTowns;

		[Inject]
		private ILocalStorageService? LocalStorage { get; set; }

		public override bool CanUpdate(string model, Version? version)
		{
			return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(TradePricesModel)}\"")
				&& IsCompatible(version, _minimumSupportedVersion);
		}

		public override Task Update(string model)
		{
			var newModel = JsonConvert.DeserializeObject<TradePricesModel>(model, new VersionConverter());
			if (newModel == null)
			{
				return Task.CompletedTask;
			}

			if (_tradePricesModel != null
				&& newModel.Goods.SequenceEqual(_tradePricesModel.Goods))
			{
				// Do not update if nothing has changed
				return Task.CompletedTask;
			}

			_tradePricesModel = newModel;

			// Build list of unique towns
			if (_availableTowns == null && _tradePricesModel.Goods.Any())
			{
				var towns = new HashSet<string>();
				foreach (var good in _tradePricesModel.Goods)
				{
					towns.Add(good.LowestPriceTown);
					towns.Add(good.HighestPriceTown);
				}
				_availableTowns = towns.OrderBy(t => t).ToList();
			}

			StateHasChanged();
			return Task.CompletedTask;
		}

		public override async Task ResetAsync()
		{
			_tradePricesModel = null;
			_selectedTown = null;
			_availableTowns = null;
			await LocalStorage!.RemoveItemAsync(SelectedTownKey);

			StateHasChanged();
			return;
		}

		protected override async Task OnInitializedAsync()
		{
			_selectedTown = await LocalStorage!.GetItemAsync<string>(SelectedTownKey);
			await base.OnInitializedAsync();
		}

		private async Task TownFilterChanged(string? newTown)
		{
			_selectedTown = newTown;
			await LocalStorage!.SetItemAsync(SelectedTownKey, _selectedTown ?? "");

			// Send filter message to server
			var filterMessage = new TradePricesFilterModel
			{
				SelectedTown = _selectedTown ?? "",
				Version = new Version("0.5.1")
			};
			var json = JsonConvert.SerializeObject(filterMessage, new VersionConverter());
			OnMessageSent(json);
		}

		private string GetActionText(TradeGoodItem good)
		{
			if (string.IsNullOrEmpty(_selectedTown)) return "";

			if (good.LowestPriceTown == _selectedTown)
			{
				return "BUY HERE";
			}
			else if (good.HighestPriceTown == _selectedTown)
			{
				return "SELL HERE";
			}
			return "";
		}

		private string GetActionClass(TradeGoodItem good)
		{
			if (string.IsNullOrEmpty(_selectedTown)) return "";

			if (good.LowestPriceTown == _selectedTown)
			{
				return "badge badge-primary";
			}
			else if (good.HighestPriceTown == _selectedTown)
			{
				return "badge badge-success";
			}
			return "";
		}

		private string GetProfitClass(int profit)
		{
			if (profit >= 100) return "text-success font-weight-bold";
			if (profit >= 50) return "text-success";
			if (profit >= 20) return "text-warning";
			return "text-muted";
		}
	}
}
