using System;

namespace Ed.Bannerboard.Models
{
    /// <summary>
    /// A model for sending information about the mod to the dashboard.
    /// </summary>
    [Serializable]
    public class HandshakeModel
    {
        /// <summary>
        /// Mod version.
        /// </summary>
        public Version Version { get; set; }
    }
}
