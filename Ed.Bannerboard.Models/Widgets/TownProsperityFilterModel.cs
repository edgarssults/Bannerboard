using System;

namespace Ed.Bannerboard.Models.Widgets
{
    /// <summary>
    /// Model for changing what data the town prosperity widget returns.
    /// </summary>
    [Serializable]
    public class TownProsperityFilterModel : IMessageModel
    {
        /// <summary>
        /// Number of towns to show in the widget.
        /// </summary>
        public int TownCount { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        public string Type => nameof(TownProsperityFilterModel);

        /// <summary>
        /// Model version.
        /// </summary>
        public Version Version { get; set; }
    }
}
