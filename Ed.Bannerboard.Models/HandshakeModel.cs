using System;

namespace Ed.Bannerboard.Models
{
    /// <summary>
    /// A model for sending information about the mod to the dashboard.
    /// </summary>
    [Serializable]
    public class HandshakeModel : IMessageModel
    {
        /// <summary>
        /// Model type.
        /// </summary>
        public string Type => nameof(HandshakeModel);

        /// <summary>
        /// Mod version.
        /// </summary>
        public Version Version { get; set; }
    }
}
