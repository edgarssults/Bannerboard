using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Model for Kingdom Wars widget data.
    /// </summary>
    [Serializable]
    public class KingdomWarsModel
    {
        /// <summary>
        /// List of kingdoms.
        /// </summary>
        public List<KingdomWarsItem> Kingdoms { get; set; }

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Single kingdom data.
    /// </summary>
    [Serializable]
    public class KingdomWarsItem
    {
        /// <summary>
        /// Kingdom name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of kingdoms and minor factions that the kingdom is at war with.
        /// </summary>
        public List<KingdomWarsFactionItem> Wars { get; set; }

        /// <summary>
        /// Primary kingdom color as a hex string (including #).
        /// </summary>
        public string PrimaryColor { get; set; }

        /// <summary>
        /// Secondary kingdom color as a hex string (including #).
        /// </summary>
        public string SecondaryColor { get; set; }
    }

    [Serializable]
    public class KingdomWarsFactionItem
    {
        /// <summary>
        /// Faction name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the faction is a minor faction.
        /// </summary>
        public bool IsMinorFaction { get; set; }

        /// <summary>
        /// Indicates whether the faction is a kingdom faction.
        /// </summary>
        public bool IsKingdomFaction { get; set; }
    }
}
