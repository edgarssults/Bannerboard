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
        /// Names of kingdoms and minor factions that the kingdom is at war with.
        /// </summary>
        public List<string> Wars { get; set; }

        /// <summary>
        /// Primary kingdom color as a hex string (including #).
        /// </summary>
        public string PrimaryColor { get; set; }

        /// <summary>
        /// Secondary kingdom color as a hex string (including #).
        /// </summary>
        public string SecondaryColor { get; set; }
    }
}
