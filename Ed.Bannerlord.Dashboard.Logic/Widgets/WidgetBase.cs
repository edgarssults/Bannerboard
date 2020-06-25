using SuperSocket.WebSocket;
using TaleWorlds.CampaignSystem;

namespace Ed.Bannerlord.Dashboard.Logic.Widgets
{
    /// <summary>
    /// Base logic for all Bannerlord Dashboard widgets.
    /// </summary>
    public abstract class WidgetBase : CampaignBehaviorBase
    {
        /// <summary>
        /// Initializes a widget.
        /// </summary>
        /// <param name="session">The session to initialize the widget for.</param>
        public abstract void Init(WebSocketSession session);
    }
}
