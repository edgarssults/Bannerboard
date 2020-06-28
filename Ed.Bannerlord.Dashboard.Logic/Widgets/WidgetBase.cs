using SuperSocket.WebSocket;
using TaleWorlds.CampaignSystem;

namespace Ed.Bannerlord.Dashboard.Logic.Widgets
{
    /// <summary>
    /// Base logic for all Bannerlord Dashboard widgets.
    /// </summary>
    public abstract class WidgetBase : CampaignBehaviorBase
    {
        protected readonly WebSocketServer Server;

        /// <summary>
        /// Base logic for all Bannerlord Dashboard widgets.
        /// </summary>
        /// <param name="server">WebSocket server to send data to.</param>
        public WidgetBase(WebSocketServer server)
        {
            Server = server;
        }

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
