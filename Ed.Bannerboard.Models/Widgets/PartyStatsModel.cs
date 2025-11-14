using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Model for party statistics widget data.
    /// </summary>
    [Serializable]
    public class PartyStatsModel : IMessageModel
    {
        /// <summary>
        /// Food statistics.
        /// </summary>
        public FoodStats Food { get; set; }

        /// <summary>
        /// Member statistics.
        /// </summary>
        public MemberStats Members { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        public string Type => nameof(PartyStatsModel);

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Food statistics.
    /// </summary>
    [Serializable]
    public class FoodStats
    {
        /// <summary>
        /// List of food items in the party inventory.
        /// </summary>
        public List<FoodStatsItem> Items { get; set; }
    }

    /// <summary>
    /// Food item.
    /// </summary>
    [Serializable]
    public class FoodStatsItem
    {
        /// <summary>
        /// Food item name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Number of food items.
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Member statistics.
    /// </summary>
    [Serializable]
    public class MemberStats
    {
        /// <summary>
        /// Number of hero units in the party.
        /// </summary>
        public int TotalHeroes { get; set; }

        /// <summary>
        /// Number of regular units in the party.
        /// </summary>
        public int TotalRegulars { get; set; }

        /// <summary>
        /// Number of wounded hero units in the party.
        /// </summary>
        public int WoundedHeroes { get; set; }

        /// <summary>
        /// Number of wounded regular units in the party.
        /// </summary>
        public int WoundedRegulars { get; set; }

        /// <summary>
        /// Total party size.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of wounded units in the party.
        /// </summary>
        public int TotalWounded { get; set; }

        /// <summary>
        /// Maximum party size.
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// List of party members.
        /// </summary>
        public List<MemberStatsItem> Items { get; set; }
    }

    /// <summary>
    /// Member item.
    /// </summary>
    [Serializable]
    public class MemberStatsItem
    {
        /// <summary>
        /// Member description.
        /// </summary>
        /// <remarks>
        /// Should not be used in UI because it's not translated.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Number of units of this type.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Number of wounded units of this type.
        /// </summary>
        public int WoundedCount { get; set; }

        /// <summary>
        /// Indicates whether the member is infantry (doesn't have a mount).
        /// </summary>
        public bool IsInfantry { get; set; }

        /// <summary>
        /// Indicates whether the member is cavalry (has a mount).
        /// </summary>
        public bool IsCavalry { get; set; }

        /// <summary>
        /// Indicates whether the member is an archer (ranged infantry).
        /// </summary>
        public bool IsArcher { get; set; }

        /// <summary>
        /// Indicates whether the member is a mounted archer (ranged cavalry).
        /// </summary>
        public bool IsMountedArcher { get; set; }

		/// <summary>
		/// Indicates whether the member is a prisoner.
		/// </summary>
		public bool IsPrisoner { get; set; }
	}
}
