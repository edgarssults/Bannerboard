using Bogus;
using Ed.Bannerboard.Models;
using Ed.Bannerboard.Models.Widgets;
using Ed.Bannerboard.Simulator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SuperSocket.SocketBase;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ed.Bannerboard.Simulator
{
	public static class Program
	{
		// This version should be in sync with the mod
		private static readonly Version _version = new Version("0.5.2");

		private static readonly VersionConverter _versionConverter = new VersionConverter();
		private static readonly Faker _faker = new Faker();
		private static WebSocketServer _server;
		private static readonly CancellationTokenSource _disposalTokenSource = new CancellationTokenSource();

		// Widget state
		private static int _townCount = 10;
		private static List<HeroTrackerFilterItem> _trackedHeroes = new List<HeroTrackerFilterItem>();

		// Simulated shared data
		private static readonly List<SimulatedKingdom> _kingdoms = new List<SimulatedKingdom>
		{
			new SimulatedKingdom("Vlandia", "#FF0000", "#000"),
			new SimulatedKingdom("Sturgia", "#00FF00", "#000"),
			new SimulatedKingdom("Aserai", "#0000FF", "#000"),
			new SimulatedKingdom("Khuzait", "#FFFF00", "#000"),
			new SimulatedKingdom("Battania", "#FF00FF", "#000"),
			new SimulatedKingdom("Empire", "#00FFFF", "#000"),
		};
		private static readonly List<HeroTrackerReturnDataItem> _allHeroes = Enumerable
			.Range(1, 50)
			.Select(x => new HeroTrackerReturnDataItem
			{
				Id = x.ToString(),
				Name = _faker.Name.FirstName() // Names will change, but IDs remain constant
			})
			.ToList();

		public static async Task Main(string[] args)
		{
			Console.WriteLine("Starting Bannerboard simulator (SuperSocket) on ws://localhost:2020...");

			_server = new WebSocketServer();

			if (!_server.Setup(2020)) // Same approach as the mod's Init.cs
			{
				Console.WriteLine("Failed to set up WebSocket server on port 2020.");
				return;
			}

			_server.NewSessionConnected += new SessionHandler<WebSocketSession>(NewSessionConnected);
			_server.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(SessionClosed);
			_server.NewMessageReceived += new SessionHandler<WebSocketSession, string>(MessageReceived);

			if (!_server.Start())
			{
				Console.WriteLine("Failed to start WebSocket server.");
				return;
			}

			Console.WriteLine("Simulator WebSocket server started. Press ENTER to stop.");

			// Start background broadcaster
			_ = Task.Run(() => BroadcastLoop(_disposalTokenSource.Token));

			Console.ReadLine();
			_disposalTokenSource.Cancel();
			_server.Stop();
			Console.WriteLine("Simulator stopped.");
		}

		private static void NewSessionConnected(WebSocketSession session)
		{
			Console.WriteLine($"Client connected: {session.SessionID}");

			// Send handshake (same model and converter the mod uses)
			var handshake = new HandshakeModel
			{
				Version = _version
			};

			var json = JsonConvert.SerializeObject(handshake, _versionConverter);
			var bytes = Encoding.UTF8.GetBytes(json);
			session.Send(new ArraySegment<byte>(bytes));
			Console.WriteLine("Handshake sent to client.");

			// Send initial widget updates immediately
			SendAllWidgetUpdates(session);
		}

		private static void SessionClosed(WebSocketSession session, CloseReason reason)
		{
			Console.WriteLine($"Client disconnected: {session.SessionID}, reason: {reason}");
		}

		private static void MessageReceived(WebSocketSession session, string message)
		{
			Console.WriteLine($"Received from {session.SessionID}: {message}");

			try
			{
				// Handle TownProsperityFilterModel (widgets use this pattern in UI/mod)
				if (Regex.IsMatch(message, $"\"Type\":.*\"{nameof(TownProsperityFilterModel)}\""))
				{
					var model = JsonConvert.DeserializeObject<TownProsperityFilterModel>(message, _versionConverter);
					if (model != null)
					{
						_townCount = model.TownCount;
						Console.WriteLine($"Town prosperity filter set to {_townCount} (from client {session.SessionID})");

						// Send a single immediate update to that session
						SendModel(session, GenerateTownProsperityModel(_townCount));
					}
				}
				else if (Regex.IsMatch(message, $"\"Type\":.*\"{nameof(HeroTrackerFilterModel)}\""))
				{
					var model = JsonConvert.DeserializeObject<HeroTrackerFilterModel>(message, _versionConverter);
					if (model != null)
					{
						_trackedHeroes = model.TrackedHeroes;
						Console.WriteLine($"Tracked heroes set to {_trackedHeroes.Count} (from client {session.SessionID})");

						// Send a single immediate update to that session
						SendModel(session, GenerateHeroTrackerModel(_trackedHeroes));
					}
				}
				else
				{
					Console.WriteLine($"Unknown message received: {message}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error handling message: {ex.Message}");
			}
		}

		private static async Task BroadcastLoop(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					// Create a full round of widget updates and broadcast them
					// Broadcast in short bursts with slight delays to simulate real mod behavior

					BroadcastModel(GenerateKingdomStrengthModel());
					await Task.Delay(150, token);

					BroadcastModel(GenerateKingdomLordsModel());
					await Task.Delay(150, token);

					BroadcastModel(GenerateKingdomWarsModel());
					await Task.Delay(150, token);

					BroadcastModel(GeneratePartyStatsModel());
					await Task.Delay(150, token);

					BroadcastModel(GenerateTownProsperityModel(_townCount));
					await Task.Delay(150, token);

					BroadcastModel(GenerateHeroTrackerModel(_trackedHeroes));
					await Task.Delay(2000, token);
				}
				catch (OperationCanceledException)
				{
					// Shutdown requested
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"BroadcastLoop error: {ex.Message}");
				}
			}
		}

		private static void BroadcastModel(object model)
		{
			if (_server == null)
			{
				return;
			}

			var json = JsonConvert.SerializeObject(model, _versionConverter);
			var bytes = Encoding.UTF8.GetBytes(json);
			var segment = new ArraySegment<byte>(bytes);

			var sessions = _server.GetAllSessions().ToList();
			foreach (var s in sessions)
			{
				try
				{
					s.Send(segment);
				}
				catch
				{
					// Ignore
				}
			}

			Console.WriteLine($"Broadcasted: {model.GetType().Name} to {sessions.Count} session(s).");
		}

		private static void SendAllWidgetUpdates(WebSocketSession session)
		{
			SendModel(session, GenerateKingdomStrengthModel());
			SendModel(session, GenerateKingdomLordsModel());
			SendModel(session, GenerateKingdomWarsModel());
			SendModel(session, GeneratePartyStatsModel());
			SendModel(session, GenerateTownProsperityModel(_townCount));
			SendModel(session, GenerateHeroTrackerModel(_trackedHeroes));
			SendModel(session, GetHeroTrackerReturnDataModel());
		}

		private static void SendModel(WebSocketSession session, object model)
		{
			try
			{
				var json = JsonConvert.SerializeObject(model, _versionConverter);
				var bytes = Encoding.UTF8.GetBytes(json);
				session.Send(new ArraySegment<byte>(bytes));
			}
			catch
			{
				// Ignore
			}
		}

		private static KingdomStrengthModel GenerateKingdomStrengthModel()
		{
			return new KingdomStrengthModel
			{
				Version = _version,
				Kingdoms = _kingdoms.Select(x => new KingdomStrengthItem
				{
					Name = x.Name,
					Strength = _faker.Random.Number(20, 220),
					PrimaryColor = x.PrimaryColor,
					SecondaryColor = x.SecondaryColor
				}).ToList()
			};
		}

		private static KingdomLordsModel GenerateKingdomLordsModel()
		{
			return new KingdomLordsModel
			{
				Version = _version,
				Kingdoms = _kingdoms.Select(x => new KingdomLordsItem
				{
					Name = x.Name,
					Lords = _faker.Random.Number(1, 30),
					PrimaryColor = x.PrimaryColor,
					SecondaryColor = x.SecondaryColor
				}).ToList()
			};
		}

		private static KingdomWarsModel GenerateKingdomWarsModel()
		{
			var wars = new List<KingdomWarsItem>();

			foreach (var kingdom in _kingdoms)
			{
				wars.Add(new KingdomWarsItem
				{
					Name = kingdom.Name,
					PrimaryColor = kingdom.PrimaryColor,
					SecondaryColor = kingdom.SecondaryColor,
					Wars =_faker.Random.Bool()
						? new List<KingdomWarsFactionItem>
						{
							new KingdomWarsFactionItem
							{
								Name = _faker.PickRandom(_kingdoms.Where(k => k.Name != kingdom.Name).ToList()).Name,
								IsKingdomFaction = true,
								IsMinorFaction = false
							}
						}
						: new List<KingdomWarsFactionItem>()
				});
			}

			return new KingdomWarsModel
			{
				Version = _version,
				Kingdoms = wars
			};
		}

		private static PartyStatsModel GeneratePartyStatsModel()
		{
			var troops = new List<MemberStatsItem>
			{
				new MemberStatsItem
				{
					Description = "Infantry",
					IsInfantry = true,
					Count = _faker.Random.Number(0, 100),
					WoundedCount = _faker.Random.Number(0, 20)
				},
				new MemberStatsItem
				{
					Description = "Archers",
					IsArcher = true,
					Count = _faker.Random.Number(0, 100),
					WoundedCount = _faker.Random.Number(0, 20)
				},
				new MemberStatsItem
				{
					Description = "Prisoner",
					IsPrisoner = true,
					IsInfantry = true,
					Count = _faker.Random.Number(0, 20),
					WoundedCount = _faker.Random.Number(0, 10)
				}
			};

			return new PartyStatsModel
			{
				Version = _version,
				Food = new FoodStats
				{
					Items = new List<FoodStatsItem>
					{
						new FoodStatsItem
						{
							Name = "Bread",
							Count = _faker.Random.Number(0, 500)
						}
					}
				},
				Members = new MemberStats
				{
					MaxCount = troops.Sum(x => x.Count) + 50,
					TotalHeroes = 0,
					TotalRegulars = troops.Sum(x => x.Count),
					Items = troops
				}
			};
		}

		private static TownProsperityModel GenerateTownProsperityModel(int townCount)
		{
			var towns = new List<TownProsperityItem>();

			for (int i = 0; i < townCount; i++)
			{
				var kingdom = _faker.PickRandom(_kingdoms);

				towns.Add(new TownProsperityItem
				{
					Name = $"Town {i + 1}",
					Prosperity = _faker.Random.Number(10, 500),
					Militia = _faker.Random.Number(0, 200),
					Garrison = _faker.Random.Number(0, 500),
					FactionName = kingdom.Name,
					PrimaryColor = kingdom.PrimaryColor,
					SecondaryColor = kingdom.SecondaryColor
				});
			}

			return new TownProsperityModel
			{
				Version = _version,
				Towns = towns.OrderByDescending(x => x.Prosperity).ToList()
			};
		}

		private static HeroTrackerReturnDataModel GetHeroTrackerReturnDataModel()
		{
			return new HeroTrackerReturnDataModel
			{
				Version = _version,
				Heroes = _allHeroes
			};
		}

		private static HeroTrackerModel GenerateHeroTrackerModel(List<HeroTrackerFilterItem> trackedHeroes)
		{
			return new HeroTrackerModel
			{
				Version = _version,
				Heroes = trackedHeroes
					.Select(x => new HeroTrackerItem
					{
						Id = x.Id,
						Name = _allHeroes.Single(y => y.Id == x.Id).Name,
						Location = _faker.Random.Bool() ? _faker.Address.City() : "-",
						IsShownOnMap = x.IsShownOnMap
					})
					.ToList()
			};
		}
	}
}