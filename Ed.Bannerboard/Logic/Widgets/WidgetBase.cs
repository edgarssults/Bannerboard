using Newtonsoft.Json.Converters;
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
		/// Default model version converter.
		/// </summary>
		protected static readonly VersionConverter DefaultVersionConverter = new VersionConverter();

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
        /// Determines whether the widget can handle a received message.
        /// </summary>
        /// <param name="message">The message that was sent.</param>
        public abstract bool CanHandleMessage(string message);

        /// <summary>
        /// Handles a received message.
        /// </summary>
        /// <param name="session">The session that the message was sent from.</param>
        /// <param name="message">The message that was sent.</param>
        public abstract void HandleMessage(WebSocketSession session, string message);

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
