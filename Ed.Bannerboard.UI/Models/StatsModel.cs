using System;

namespace Ed.Bannerboard.UI.Models
{
    public class StatsModel
    {
        /// <summary>
        /// Number of received messages by the client from the server.
        /// </summary>
        public int ReceivedMessageCount { get; set; }

        /// <summary>
        /// Size in bytes of the last received message.
        /// </summary>
        public long LastMessageBytes { get; set; }

        /// <summary>
        /// Connected client mod version.
        /// </summary>
        public Version? ModVersion { get; set; }

        /// <summary>
        /// Dashboard UI version.
        /// </summary>
        public Version? DashboardVersion { get; set; }
    }
}
