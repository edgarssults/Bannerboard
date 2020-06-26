using System;
using System.Collections.Generic;

namespace Ed.Bannerlord.Dashboard.Models.Widgets
{
    /// <summary>
    /// Model for Kingdom Strength widget data.
    /// </summary>
    [Serializable]
    public class KingdomStrengthModel
    {
        /// <summary>
        /// List of kingdoms.
        /// </summary>
        public List<KingdomStrengthItem> Kingdoms { get; set; }
    }

    /// <summary>
    /// Single kingdom data.
    /// </summary>
    [Serializable]
    public class KingdomStrengthItem
    {
        /// <summary>
        /// Kingdom name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Kingdom strength.
        /// </summary>
        public float Strength { get; set; }

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
