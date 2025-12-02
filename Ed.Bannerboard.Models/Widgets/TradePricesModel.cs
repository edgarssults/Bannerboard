using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
	/// <summary>
	/// Model for the trade prices widget data.
	/// </summary>
	[Serializable]
	public class TradePricesModel : IMessageModel
	{
		/// <summary>
		/// List of tradeable goods with price data.
		/// </summary>
		public List<TradeGoodItem> Goods { get; set; }

		/// <summary>
		/// Model type.
		/// </summary>
		public string Type => nameof(TradePricesModel);

		/// <summary>
		/// Model version.
		/// </summary>
		public Version Version { get; set; }
	}

	/// <summary>
	/// Single trade good data.
	/// </summary>
	[Serializable]
	public class TradeGoodItem
	{
		/// <summary>
		/// Name of the trade good.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Highest selling price in the game.
		/// </summary>
		public int HighestPrice { get; set; }

		/// <summary>
		/// Town where the highest price is found.
		/// </summary>
		public string HighestPriceTown { get; set; }

		/// <summary>
		/// Lowest selling price in the game.
		/// </summary>
		public int LowestPrice { get; set; }

		/// <summary>
		/// Town where the lowest price is found.
		/// </summary>
		public string LowestPriceTown { get; set; }

		/// <summary>
		/// Profit margin (difference between highest and lowest).
		/// </summary>
		public int ProfitMargin { get; set; }
	}
}
