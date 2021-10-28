using Ed.Bannerboard.Logic.Widgets;
using SuperSocket.SocketBase;
using SuperSocket.WebSocket;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Ed.Bannerboard
{
    public class Init : MBSubModuleBase
    {
        private WebSocketServer _server = new WebSocketServer();
        private List<WidgetBase> _widgets = new List<WidgetBase>();

        /// <summary>
        /// Handles game starting.
        /// </summary>
        /// <param name="game">The game that started.</param>
        /// <param name="gameStarter">Game starting logic.</param>
        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign)
            {
                StartServer();

                // Define dashboard widgets
                _widgets.Add(new KingdomStrengthWidget(_server));
                _widgets.Add(new KingdomLordsWidget(_server));
                _widgets.Add(new KingdomWarsWidget(_server));

                // Register widget behaviors in the game
                var campaignStarter = gameStarter as CampaignGameStarter;
                _widgets.ForEach(w => campaignStarter.AddBehavior(w));
            }
        }

        /// <summary>
        /// Handles game ending.
        /// </summary>
        /// <param name="game">The game that ended.</param>
        public override void OnGameEnd(Game game)
        {
            if (game.GameType is Campaign && _server != null)
            {
                StopServer();
                _widgets = null;
            }
        }

        /// <summary>
        /// Starts the WebSocket server.
        /// </summary>
        private void StartServer()
        {
            if (!_server.Setup(2020)) // TODO: Change the port or use an URL?
            {
                InformationManager.DisplayMessage(new InformationMessage("Bannerboard server set-up failed!"));
                return;
            }

            _server.NewSessionConnected += new SessionHandler<WebSocketSession>(NewSessionConnected);
            _server.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(SessionClosed);

            if (!_server.Start())
            {
                InformationManager.DisplayMessage(new InformationMessage("Bannerboard server start-up failed!"));
                return;
            }

            InformationManager.DisplayMessage(new InformationMessage("Bannerboard server started"));
        }

        /// <summary>
        /// Stops the WebSocket server.
        /// </summary>
        private void StopServer()
        {
            // TODO: Better disconnection update in UI

            _server.Stop();
            _server = null;
            InformationManager.DisplayMessage(new InformationMessage("Bannerboard server stopped"));
        }

        /// <summary>
        /// Handles session creation event.
        /// </summary>
        /// <param name="session">The created session.</param>
        private void NewSessionConnected(WebSocketSession session)
        {
            InformationManager.DisplayMessage(new InformationMessage("Bannerboard client connected"));

            // Initialize all widgets
            // TODO: Check which widgets and versions the dashboard supports, only update about those
            _widgets.ForEach(w =>
            {
                try
                {
                    w.Init(session);
                }
                catch
                {
                    InformationManager.DisplayMessage(new InformationMessage($"Error while initializing {w.GetType().FullName}!"));
                }
            });
        }

        /// <summary>
        /// handles session closure event.
        /// </summary>
        /// <param name="session">The closed session.</param>
        /// <param name="reason">Closure reason.</param>
        private void SessionClosed(WebSocketSession session, CloseReason reason)
        {
            InformationManager.DisplayMessage(new InformationMessage("Bannerboard client disconnected"));
        }
    }
}
