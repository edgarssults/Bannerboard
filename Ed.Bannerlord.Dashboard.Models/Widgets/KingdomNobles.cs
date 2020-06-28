using System;
using System.Collections.Generic;

namespace Ed.Bannerlord.Dashboard.Models.Widgets
{
    /// <summary>
    /// Model for Kingdom Nobles widget data.
    /// </summary>
    [Serializable]
    public class KingdomNoblesModel
    {
        /// <summary>
        /// List of kingdoms.
        /// </summary>
        public List<KingdomNoblesItem> Kingdoms { get; set; }
    }

    /// <summary>
    /// Single kingdom data.
    /// </summary>
    [Serializable]
    public class KingdomNoblesItem
    {
        /// <summary>
        /// Kingdom name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Kingdom noble count.
        /// </summary>
        public int Nobles { get; set; }

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
