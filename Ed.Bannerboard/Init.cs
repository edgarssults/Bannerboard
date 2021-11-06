using Ed.Bannerboard.Logic;
using Ed.Bannerboard.Logic.Widgets;
using Ed.Bannerboard.Models;
using SuperSocket.SocketBase;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Ed.Bannerboard
{
    public class Init : MBSubModuleBase
    {
        // This version should be in sync with the version in SubModule.xml
        private readonly Version _version = new Version("0.2.1");

        private WebSocketServer _server;
        private List<WidgetBase> _widgets;

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
                _widgets = new List<WidgetBase>();
                _widgets.Add(new KingdomStrengthWidget(_server, _version));
                _widgets.Add(new KingdomLordsWidget(_server, _version));
                _widgets.Add(new KingdomWarsWidget(_server, _version));

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

                // Clear all loaded widgets
                _widgets = null;
            }
        }

        /// <summary>
        /// Starts the WebSocket server.
        /// </summary>
        private void StartServer()
        {
            _server = new WebSocketServer();

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

            // Send handshake
            var model = new HandshakeModel
            {
                Version = _version,
            };
            session.Send(new ArraySegment<byte>(model.ToByteArray()));

            // Initialize all widgets
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
        /// Handles session closure event.
        /// </summary>
        /// <param name="session">The closed session.</param>
        /// <param name="reason">Closure reason.</param>
        private void SessionClosed(WebSocketSession session, CloseReason reason)
        {
            InformationManager.DisplayMessage(new InformationMessage("Bannerboard client disconnected"));
        }
    }
}
