using System;

namespace Ed.Bannerboard.Models.Widgets
{
	/// <summary>
	/// Filter model for trade prices widget.
	/// </summary>
	[Serializable]
	public class TradePricesFilterModel : IMessageModel
	{
		/// <summary>
		/// Selected town name to filter by (null or empty for all towns).
		/// </summary>
		public string SelectedTown { get; set; }

		/// <summary>
		/// Model type.
		/// </summary>
		public string Type => nameof(TradePricesFilterModel);

		/// <summary>
		/// Model version.
		/// </summary>
		public Version Version { get; set; }
	}
}
