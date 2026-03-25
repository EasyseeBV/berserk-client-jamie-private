using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.Duels;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UI;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelsApplication : IDuelsApplication, IDisposable
	{
		private readonly ISharedConfig sharedConfig;
		private readonly IGameDatabase gameDatabase;
		private readonly IDeckApplication deckApplication;
		private readonly IDuelJoinApplication duelJoinApplication;
		private readonly IDuelRoomApplication duelRoomApplication;
		private readonly IDuelCreateApplication duelCreateApplication;
		private readonly IDuelSelectDeckApplication duelSelectDeckApplication;
		private readonly ICustomisationItemRepository customisationsRepository;
		private readonly ISignalTimeoutProcessor signalTimeoutProcessor;
		private readonly IDuelsSignalProcessor duelsSignalProcessor;
		
		private static LobbyDuelView LobbyDuelView => LobbyDuelView.Instance;
		private readonly List<DuelRoomItemData> roomItemDatas = new();
		
		public DuelsApplication(
			ISharedConfig sharedConfig,
			IGameDatabase gameDatabase,
			IDeckApplication deckApplication,
			IDuelJoinApplication duelJoinApplication,
			IDuelRoomApplication duelRoomApplication,
			IDuelCreateApplication duelCreateApplication,
			IDuelSelectDeckApplication duelSelectDeckApplication,
			ICustomisationItemRepository customisationsRepository,
			ISignalTimeoutProcessor signalTimeoutProcessor,
			IDuelsSignalProcessor duelsSignalProcessor)
		{
			this.sharedConfig = sharedConfig;
			this.gameDatabase = gameDatabase;
			this.duelJoinApplication = duelJoinApplication;
			this.duelRoomApplication = duelRoomApplication;
			this.duelCreateApplication = duelCreateApplication;
			this.duelSelectDeckApplication = duelSelectDeckApplication;
			this.customisationsRepository = customisationsRepository;
			this.signalTimeoutProcessor = signalTimeoutProcessor;
			this.duelsSignalProcessor = duelsSignalProcessor;
			this.deckApplication = deckApplication;
		}
		
		public void Dispose()
		{
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			signalTimeoutProcessor.Clear();
		}

		public async UniTask OpenAsync()
		{
			if (LobbyDuelView.VisibleState == VisibleState.Visible)
				return;
			
			LobbyDuelView
				.SubscribeOnCreateRoom(duelCreateApplication.Open)
				.SubscribeOnJoinRoom(() => duelJoinApplication.Open(null, true))
				.SubscribeOnSelectDeck(() => duelSelectDeckApplication.OpenAsync(OnRefreshUserInfo).Forget())
				.SubscribeOnPractice(() => StartPracticeAsync().AddLoadingTask().Forget())
				.SubscribeOnBack(Close)
				.SetFilter(FilterRooms);
			
			await UniTask.WhenAll(FetchAsyc(), RefreshUserInfoAsync()).AddLoadingTask();
			duelsSignalProcessor.OnUpdateReceived += OnMessageReceived;
			LobbyDuelView.Show();
		}
		
		public void Close()
		{
			if (LobbyDuelView.VisibleState == VisibleState.Visible)
				LobbyDuelView.Close();
			
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;

			if (!roomItemDatas.Any()) 
				return;
			
			roomItemDatas.ForEach(x => x.Dispose());
			roomItemDatas.Clear();
		}

		private async UniTask FetchAsyc()
		{
			var duelAvailableRooms = await LobbyAPI.GetDuelAvailableRooms();
			var newRooms = duelAvailableRooms.Data.MapDuelRooms(sharedConfig.PlayerLimit);
			var oldRooms = roomItemDatas.ToArray();
			roomItemDatas.Clear();
				
			oldRooms.ForEach(RemoveRoom);
			newRooms.ForEach(AddOrRefreshRoom);
		}
		
		private async UniTask StartPracticeAsync()
		{
			if (deckApplication.Current == null)
			{
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_ValidateDeck_NoDeckSelected"));
				return;
			}

			var model = new LobbyPracticeStartSessionModel
			{
				MatchMode = MatchMode.Practice,
				Difficulty = PracticeMode.Hard,
				DeckId = deckApplication.Current.Id
			};
			var response = await LobbyAPI.PostPracticeStartSession(model);
			if (!response)
			{
				NotifyClientException(response.GetMessage());
				return;
			}
			
			if (!await signalTimeoutProcessor.WaitResponseAsync(LobbySessionsAction.ActiveSession, true))
			{
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_PracticeDuelFailed_NoResponse"),
					"Refresh", () => StartPracticeAsync().AddLoadingTask().Forget(), true);
			}
		}
		
		private UniTask RefreshUserInfoAsync()
		{
			var currentDeck = deckApplication.Current;
			var ownedVulcanite = currentDeck == null
				? null
				: User.OwnedVulcanites.FirstOrDefault(x => x.Id == currentDeck.OwnedVulcaniteId);
			var heroData = gameDatabase.GetHero(ownedVulcanite?.VulcaniteId);
			var frameUrl = customisationsRepository.GetFirstEquipped(CustomisationType.AvatarFrame)?.PreviewURL;
			var avatarUrl = heroData?.ArtUrl;
			var userName = User.UserName.Ellipsis(16);
			return LobbyDuelView.BuildAsync(userName, avatarUrl, frameUrl);
		}
		
		private void OnRefreshUserInfo()
		{
			RefreshUserInfoAsync().AddLoadingTask().Forget();
		}
		
		private void OnDuelRoomClicked(DuelRoomItemData duelData)
		{
			if (duelData.IsLocked)
			{
				duelJoinApplication.Open(duelData.Id);
				return;
			}

			duelJoinApplication.JoinAsync(duelData.Id, duelData.RoomCode, null)
				.AddLoadingTask()
				.Forget();
		}
		
		private bool FilterRooms(string id, string query)
		{
			if (string.IsNullOrEmpty(query))
				return true;
			
			var roomData = roomItemDatas.FirstOrDefault(x => x.Id == id);
			if (roomData == null)
				return false;
			
			return roomData.RoomSlotsText.Trim().ToLower().Contains(query)
			       || roomData.NameText.Trim().ToLower().Contains(query);
		}

		private void AddOrRefreshRoom(DuelRoomItemData duelRoomData)
		{
			roomItemDatas.RemoveAll(x =>
			{
				if (x.Id != duelRoomData.Id) 
					return false;

				duelRoomData.Order = Math.Max(duelRoomData.Order, x.Order);
				x.Dispose();
				return true;
			});
			
			var insertIndex = Math.Clamp(duelRoomData.Order, 0, roomItemDatas.Count);
			duelRoomData.SetSelectedAction(OnDuelRoomClicked);
			roomItemDatas.Insert(insertIndex, duelRoomData);
			LobbyDuelView.AddOrRefresh(duelRoomData);
			LobbyDuelView.ApplyFilter();
		}

		private void RemoveRoom(DuelRoomItemData duelRoomData)
		{
			roomItemDatas.RemoveAll(x =>
			{
				if (x.Id != duelRoomData.Id) 
					return false;
						
				x.Dispose();
				return true;
			});
					
			LobbyDuelView.Delete(duelRoomData);
			LobbyDuelView.ApplyFilter();
		}
		
		private async UniTask<bool> TryJoinRoomAsync(DuelRoomItemData duelRoomData)
		{
			if (duelRoomData.Players.All(x => x.UserId != User.Id))
				return false;

			await duelRoomApplication.OpenAsync(duelRoomData, () => OpenAsync().Forget()).AddLoadingTask();
			Close();

			return true;
		}
		
		private async void OnMessageReceived(LobbyDuelAction action, LobbyDuelRoomModel duelRoomModel)
		{
			switch (action)
			{
				case LobbyDuelAction.Created or LobbyDuelAction.PlayerJoined: // added or changed or redirect to room
				{
					var duelRoomData = duelRoomModel.MapDuelRoom(sharedConfig.PlayerLimit, 0);
					signalTimeoutProcessor.ResponseReceived(action);
					
					if (!await TryJoinRoomAsync(duelRoomData))
						AddOrRefreshRoom(duelRoomData);
					
					return;
				}
				case LobbyDuelAction.Closed or LobbyDuelAction.Started: // removed
				{
					var duelRoomData = duelRoomModel.MapDuelRoom(sharedConfig.PlayerLimit, 0);
					signalTimeoutProcessor.ResponseReceived(action);
					RemoveRoom(duelRoomData);
					return;
				}
				case LobbyDuelAction.PlayerLeft: // removed or changed
				{
					var duelRoomData = duelRoomModel.MapDuelRoom(sharedConfig.PlayerLimit, 0);
					signalTimeoutProcessor.ResponseReceived(action);
					if (duelRoomData.Players is not {Count: > 0} || duelRoomData.IsClosed || duelRoomData.IsStarted)
					{
						RemoveRoom(duelRoomData);
						return;
					}
					
					AddOrRefreshRoom(duelRoomData);
					return;
				}
				
				default: return;
			}
		}
		
		private static void NotifyClientException(string message, string okText = "OK", Action repeatAction = null, bool cancel = false)
		{
			ConfirmationDialog.Instance.Init()
				.SetTitle("Information")
				.SetMessage(message)
				.SetResponseOk(repeatAction)
				.SetOk(okText)
				.SetCancel(cancel ? "Cancel" : null)
				.Apply();
		}
	}
}
