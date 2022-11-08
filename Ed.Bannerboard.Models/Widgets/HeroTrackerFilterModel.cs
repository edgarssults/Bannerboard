using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Model for changing which heroes the hero tracker widget tracks.
    /// </summary>
    [Serializable]
    public class HeroTrackerFilterModel : IMessageModel
    {
        /// <summary>
        /// List of heroes to track.
        /// </summary>
        public List<HeroTrackerFilterItem> TrackedHeroes { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        public string Type => nameof(HeroTrackerFilterModel);

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Single hero data.
    /// </summary>
    [Serializable]
    public class HeroTrackerFilterItem
    {
        /// <summary>
        /// Hero identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Indicates whether the last known hero location should be marked on the map.
        /// </summary>
        public bool IsShownOnMap { get; set; }
    }
}
