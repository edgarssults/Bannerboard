using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Model for the town prosperity widget data.
    /// </summary>
    [Serializable]
    public class TownProsperityModel : IMessageModel
    {
        /// <summary>
        /// List of kingdoms.
        /// </summary>
        public List<TownProsperityItem> Towns { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        public string Type => nameof(TownProsperityModel);

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Single town data.
    /// </summary>
    [Serializable]
    public class TownProsperityItem
    {
        /// <summary>
        /// Town name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Town prosperity.
        /// </summary>
        public float Prosperity { get; set; }

        /// <summary>
        /// Primary town owner color as a hex string (including #).
        /// </summary>
        public string PrimaryColor { get; set; }

        /// <summary>
        /// Secondary town owner color as a hex string (including #).
        /// </summary>
        public string SecondaryColor { get; set; }
    }
}
