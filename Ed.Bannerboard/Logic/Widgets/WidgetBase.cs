using SuperSocket.WebSocket;
using System;
using TaleWorlds.CampaignSystem;

namespace Ed.Bannerboard.Logic.Widgets
{
    /// <summary>
    /// Base logic for all Bannerboard widgets.
    /// </summary>
    public abstract class WidgetBase : CampaignBehaviorBase
    {
        /// <summary>
        /// Base logic for all Bannerboard widgets.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        /// <param name="version">Mod version.</param>
        public WidgetBase(WebSocketServer server, Version version)
        {
            Server = server;
            Version = version;
        }

        /// <summary>
        /// WebSocket server to send data to.
        /// </summary>
        protected WebSocketServer Server { get; set; }

        /// <summary>
        /// Widget version.
        /// </summary>
        protected Version Version { get; set; }

        /// <summary>
        /// Initializes a widget.
        /// </summary>
        /// <param name="session">The session to initialize the widget for.</param>
        public abstract void Init(WebSocketSession session);

        /// <summary>
        /// Syncs widget save data.
        /// </summary>
        /// <param name="dataStore">Data store.</param>
        public override void SyncData(IDataStore dataStore)
        {
            // Nothing to sync
        }
    }
}
