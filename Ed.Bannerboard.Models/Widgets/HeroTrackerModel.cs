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
        /// Indicates whether the hero is dead and can be removed from the tracker.
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Indicates whether the hero is disabled.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Indicates whether the last known hero location is marked on the map.
        /// </summary>
        public bool IsShownOnMap { get; set; }

		public override bool Equals(object obj)
		{
			return obj is HeroTrackerItem item
				&& Id == item.Id
				&& Name == item.Name
				&& Location == item.Location
				&& IsDead == item.IsDead
				&& IsDisabled == item.IsDisabled
				&& IsShownOnMap == item.IsShownOnMap;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
