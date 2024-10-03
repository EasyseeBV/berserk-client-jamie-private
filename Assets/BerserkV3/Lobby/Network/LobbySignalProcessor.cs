using System;
using System.Linq;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.SignalR.Common;
using Berserk.Shared.SignalR.Enums;
using Berserk.Shared.SignalR.Lobby;
using BerserkV3.Common.Network;
using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Deck;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using Newtonsoft.Json;
using RR.Core.Extensions;
using UI;
using Zenject;

namespace BerserkV3.Lobby.Network
{
	public class LobbySignalProcessor : IInitializable, IDisposable
	{
		private readonly ILobbyHub lobbyHub;
		public LobbySignalProcessor(ILobbyHub lobbyHub)
		{
			this.lobbyHub = lobbyHub;
		}

		public void Initialize()
		{
			lobbyHub.OnMessageReceived -= Process;
			lobbyHub.OnMessageReceived += Process;
		}
		
		public void Dispose()
		{
			lobbyHub.OnMessageReceived -= Process;
		}
		
		private void Process(string target, object[] args)
		{
			if (!Enum.TryParse(target, out SignalType signalType))
				return;
			
			var argumentString = args?.FirstOrDefault()?.ToString() ?? string.Empty;
			
			switch (signalType)
			{
				case SignalType.Information:
				{
					var infoMessage = JsonConvert.DeserializeObject<InfoMessage>(argumentString);
					LobbyBus.CurrentOnlineCount += infoMessage!.OnlinePlayers.Count;
					break;
				}

				case SignalType.Reauthorization:
				{
					var reauthSignal = JsonConvert.DeserializeObject<ReauthorizeSignal>(argumentString);
					LobbyBus.OnReauthorizationRequired.Publish(reauthSignal.Message);
					break;
				}

				case SignalType.RefreshToken:
				{
					AuthTokenRefresher.RefreshAuthTokenAsync(User.RefreshToken).Forget(DefaultSharedLogger.Error);
					break;
				}

				case SignalType.VulcaniteRentExpired:
				{
					var rentVulcaniteDto =
						JsonConvert.DeserializeObject<SocketSignal<VulcaniteExpiredSignalDto>>(argumentString);
					if (rentVulcaniteDto == null)
						DefaultSharedLogger.Log(
							$"[{nameof(LobbyHub).Red().Bold()}] {nameof(VulcaniteExpiredSignalDto)} is not parsed");
					VulcaniteHandler.RefreshVulcaniteRents(rentVulcaniteDto!.Message.VulcaniteIds);
					break;
				}

				case SignalType.SubscriptionExpired:
				{
					var cardsExpiderDto = JsonConvert.DeserializeObject<SocketSignal<CardsExpiredSignalDto>>(argumentString);
					if (cardsExpiderDto == null)
						DefaultSharedLogger.Log($"[{nameof(LobbyHub).Red().Bold()}] {nameof(VulcaniteExpiredSignalDto)} is not parsed");

					DeckApplicationAdapter.Application.RefreshSubscriptions(cardsExpiderDto!.Message.CardIds);
					break;
				}

				case SignalType.RankedDecay:
				{
					var rankDecaySignal = JsonConvert.DeserializeObject<SocketSignal<RankedDecaySignalDto>>(argumentString);
					var rankedModel = rankDecaySignal?.Message;
					if (rankedModel == null)
					{
						DefaultSharedLogger.Log($"[{nameof(LobbyHub).Red().Bold()}] {nameof(RankedDecaySignalDto)} is not parsed");
						break;
					}

					LobbyBus.RequestRefreshLeagueStatistics.Publish();
					var decayMessage = $"Your MMR has been reduced due to a period of inactivity at " +
					                   $"{rankedModel.LeagueName} to MMR: {rankedModel.NewRanked}";
					ConfirmationDialog.Instance.Init()
						.SetMessage(decayMessage)
						.SetTitle("Information")
						.SetCancel()
						.Apply();
					break;
				}

				case SignalType.DropConnection:
				{
					lobbyHub.RestartRequired();
					break;
				}
				
				default:
					DefaultSharedLogger.Error($"[{GetType().Name.Orange().Bold()}] Not handled {nameof(SignalType)} : {signalType.ToString().Red()}");
					return;
			}
		}
	}
}