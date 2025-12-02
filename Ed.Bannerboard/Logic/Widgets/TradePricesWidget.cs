using Ed.Bannerboard.Models.Widgets;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Ed.Bannerboard.Logic.Widgets
{
	/// <summary>
	/// A widget for displaying trade prices across towns.
	/// </summary>
	public class TradePricesWidget : WidgetBase
	{
		private string _selectedTown = null;

		/// <summary>
		/// A widget for displaying trade prices.
		/// </summary>
		/// <param name="server">WebSocket server to send data to.</param>
		/// <param name="version">Mod version.</param>
		public TradePricesWidget(WebSocketServer server, Version version)
			: base(server, version)
		{
		}

		public override void RegisterEvents()
		{
			CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(() =>
			{
				foreach (var session in Server.GetAllSessions())
				{
					SendUpdate(session);
				}
			}));
		}

		public override void Init(WebSocketSession session)
		{
			SendUpdate(session);
		}

		public override bool CanHandleMessage(string message)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(message, $"\"Type\":.*\"{nameof(TradePricesFilterModel)}\"");
		}

		public override void HandleMessage(WebSocketSession session, string message)
		{
			var model = Newtonsoft.Json.JsonConvert.DeserializeObject<TradePricesFilterModel>(message, new Newtonsoft.Json.Converters.VersionConverter());
			if (model == null)
			{
				return;
			}

			// Update selected town filter
			_selectedTown = string.IsNullOrEmpty(model.SelectedTown) ? null : model.SelectedTown;

			// Send updated data
			SendUpdate(session);
		}

		private void SendUpdate(WebSocketSession session)
		{
			try
			{
				var tradeData = new Dictionary<string, List<(string town, int price)>>();

				// Get all towns with markets
				var towns = Campaign.Current.Settlements
					.Where(s => s.IsTown && s.Town != null && s.Town.MarketData != null)
					.ToList();

				if (towns.Count == 0)
				{
					InformationManager.DisplayMessage(new InformationMessage("No towns with market data found", Colors.Yellow));
					return;
				}

				// Sample items from the first town to get the list of tradeable goods
				var sampleTown = towns.First().Town;
				var tradeableItems = new List<ItemObject>();

				// Try to get items from the owner's stash
				if (sampleTown.Owner?.ItemRoster != null)
				{
					foreach (var rosterElement in sampleTown.Owner.ItemRoster)
					{
						var item = rosterElement.EquipmentElement.Item;
						if (item != null && item.ItemCategory != null && item.ItemCategory.IsTradeGood)
						{
							if (!tradeableItems.Contains(item))
							{
								tradeableItems.Add(item);
							}
						}
					}
				}

				// If we didn't find any items, skip
				if (tradeableItems.Count == 0)
				{
					InformationManager.DisplayMessage(new InformationMessage("No tradeable items found", Colors.Yellow));
					return;
				}

				// Now check prices for each item across all towns
				foreach (var item in tradeableItems)
				{
					var itemName = item.Name.ToString();

					foreach (var settlement in towns)
					{
						var price = settlement.Town.MarketData.GetPrice(item, null, false);
						if (price <= 0) continue;

						if (!tradeData.ContainsKey(itemName))
						{
							tradeData[itemName] = new List<(string, int)>();
						}

						tradeData[itemName].Add((settlement.Name.ToString(), price));
					}
				}

				// Build the model
				var goods = new List<TradeGoodItem>();

				foreach (var kvp in tradeData)
				{
					if (kvp.Value.Count < 2) continue; // Need at least 2 towns to compare prices

					var orderedPrices = kvp.Value.OrderBy(x => x.price).ToList();
					var lowest = orderedPrices.First();
					var highest = orderedPrices.Last();

					if (highest.price <= lowest.price) continue; // Skip if no price difference

					// If a town is selected, only show items where that town is either the buy or sell location
					if (!string.IsNullOrEmpty(_selectedTown))
					{
						bool townInvolved = lowest.town == _selectedTown || highest.town == _selectedTown;
						if (!townInvolved) continue;
					}

					goods.Add(new TradeGoodItem
					{
						Name = kvp.Key,
						HighestPrice = highest.price,
						HighestPriceTown = highest.town,
						LowestPrice = lowest.price,
						LowestPriceTown = lowest.town,
						ProfitMargin = highest.price - lowest.price
					});
				}

				var model = new TradePricesModel
				{
					Goods = goods.OrderByDescending(g => g.ProfitMargin).Take(50).ToList(), // Limit to top 50
					Version = Version,
				};

				session.Send(model.ToJsonArraySegment());
			}
			catch (Exception ex)
			{
				// Log the error
				InformationManager.DisplayMessage(
					new InformationMessage($"Trade Prices Error: {ex.Message}", Colors.Red)
				);
			}
		}
	}
}
