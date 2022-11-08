using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Model for the hero tracker widget data.
    /// </summary>
    [Serializable]
    public class HeroTrackerModel : IMessageModel
    {
        /// <summary>
        /// List of heroes that are being tracked.
        /// </summary>
        public List<HeroTrackerItem> Heroes { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        public string Type => nameof(HeroTrackerModel);

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Single hero data.
    /// </summary>
    [Serializable]
    public class HeroTrackerItem
    {
        /// <summary>
        /// Hero identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Hero name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Last known hero location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// How many days old is the information.
        /// </summary>
        public float DaysAgo { get; set; }

        /// <summary>
        /// Indicates whether the hero is dead and can be removed from the tracker.
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Indicates whether the hero is diabled.
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}
