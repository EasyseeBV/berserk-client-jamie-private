using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.SignalR.Common;
using Berserk.Shared.SignalR.Enums;
using Berserk.Shared.SignalR.Lobby;
using BerserkV3.Common.Network;
using BerserkV3.Lobby.Vulcanite.Abstractions;
using BerserkV3.Lobby.Decks;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using Newtonsoft.Json;
using RR.Core.Extensions;
using UI;
using Zenject;

namespace BerserkV3.Lobby.Network
{
	public class LobbySignalProcessor : IInitializable, IDisposable, ILobbySingalProcessor
	{
		public event Action<SignalType, object> OnMessageReceived;

		private readonly ILobbyHub lobbyHub;
		private readonly IVulcaniteApplication vulcaniteApplication;

		public LobbySignalProcessor(ILobbyHub lobbyHub,
			IVulcaniteApplication vulcaniteApplication)
		{
			this.lobbyHub = lobbyHub;
			this.vulcaniteApplication = vulcaniteApplication;
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

		public UniTask SendAsync(object target, object data, CancellationToken token = default)
		{
			return lobbyHub.SendAsync(target.ToString(), token, data).AsUniTask();
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
					var onlinePlayersModel = JsonConvert.DeserializeObject<OnlinePlayersModel>(argumentString);
					OnMessageReceived?.Invoke(signalType, onlinePlayersModel);
					break;
				}

				case SignalType.Reauthorization:
				{
					var reauthSignal = JsonConvert.DeserializeObject<ReauthorizeSignal>(argumentString);
					LobbyBus.OnReauthorizationRequired.Publish(reauthSignal.Message);
					break;
				}

				case SignalType.Store:
				{
					var storeSignal = JsonConvert.DeserializeObject<StoreSignal>(argumentString);
					LobbyBus.OnPurchasedRecieved.Publish(storeSignal.PurchasedItems);
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
					vulcaniteApplication.RefreshVulcaniteRents(rentVulcaniteDto!.Message.VulcaniteIds);
					break;
				}

				case SignalType.SubscriptionExpired:
				{
					var cardsExpiderDto =
						JsonConvert.DeserializeObject<SocketSignal<CardsExpiredSignalDto>>(argumentString);
					if (cardsExpiderDto == null)
						DefaultSharedLogger.Log(
							$"[{nameof(LobbyHub).Red().Bold()}] {nameof(VulcaniteExpiredSignalDto)} is not parsed");

					DeckApplicationAdapter.Application.RefreshSubscriptions(cardsExpiderDto!.Message.CardIds);
					break;
				}

				case SignalType.RankedDecay:
				{
					var rankDecaySignal =
						JsonConvert.DeserializeObject<SocketSignal<RankedDecaySignalDto>>(argumentString);
					var rankedModel = rankDecaySignal?.Message;
					if (rankedModel == null)
					{
						DefaultSharedLogger.Log(
							$"[{nameof(LobbyHub).Red().Bold()}] {nameof(RankedDecaySignalDto)} is not parsed");
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
					DefaultSharedLogger.Error(
						$"[{GetType().Name.Orange().Bold()}] Not handled {nameof(SignalType)} : {signalType.ToString().Red()}");
					return;
			}
		}
	}
}