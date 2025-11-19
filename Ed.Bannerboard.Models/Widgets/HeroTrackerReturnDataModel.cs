using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Message for returning all trackable heroes.
    /// </summary>
    [Serializable]
    public class HeroTrackerReturnDataModel : IMessageModel
    {
        /// <summary>
        /// List of heroes that are being tracked.
        /// </summary>
        public List<HeroTrackerReturnDataItem> Heroes { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        public string Type => nameof(HeroTrackerReturnDataModel);

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Single hero data.
    /// </summary>
    [Serializable]
    public class HeroTrackerReturnDataItem
    {
        /// <summary>
        /// Hero identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Hero name.
        /// </summary>
        public string Name { get; set; }

		public override bool Equals(object obj)
		{
			return obj is HeroTrackerReturnDataItem item
				&& Id == item.Id
				&& Name == item.Name;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
