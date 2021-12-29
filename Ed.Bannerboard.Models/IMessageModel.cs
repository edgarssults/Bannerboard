using System;

namespace Ed.Bannerboard.Models
{
    /// <summary>
    /// An interface that describes message models.
    /// </summary>
    public interface IMessageModel
    {
        /// <summary>
        /// Model type.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Model version.
        /// </summary>
        Version Version { get; set; }
    }
}
