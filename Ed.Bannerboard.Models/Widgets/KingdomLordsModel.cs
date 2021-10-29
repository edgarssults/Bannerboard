using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Model for Kingdom Lords widget data.
    /// </summary>
    [Serializable]
    public class KingdomLordsModel
    {
        /// <summary>
        /// List of kingdoms.
        /// </summary>
        public List<KingdomLordsItem> Kingdoms { get; set; }

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Single kingdom data.
    /// </summary>
    [Serializable]
    public class KingdomLordsItem
    {
        /// <summary>
        /// Kingdom name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Kingdom lord count.
        /// </summary>
        public int Lords { get; set; }

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
