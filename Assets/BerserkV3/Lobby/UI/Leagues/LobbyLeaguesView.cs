using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.Data.Lobby.Statistics;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using Lobby.Items;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using Sirenix.Utilities;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Leagues
{
	public partial class LobbyLeaguesView : BaseView
	{
		private static readonly int DEFAULT_MMR = 1200;

		[SerializeField] private List<GameObject> leagueSelectionObjects;
		[SerializeField] private List<RawImage> leagueImages;
		[SerializeField] private List<Button> leagueButtons;
		[SerializeField] private DeckButtonView deckButtonPrefab;
		[SerializeField] private Transform decksContainer;
		[SerializeField] private PlayerView playerView;
		[SerializeField] private TextMeshProUGUI leagueNameText;
		[SerializeField] private TextMeshProUGUI leagueDescriptionText;
		[SerializeField] private Button playButton;
		[SerializeField] private Button backButton;

		private Dictionary<string, DeckButtonView> deckItems;
		private Dictionary<string, DeckData> deckModels;
		private List<UserStatisticsLeagueData> leagueStatistics;
		
		private int currentLeagueIndex;
		private string selectedDeck;
		private bool selectedDeckValid;
		private CancellationTokenSource updateFlags;

		protected override void OnAwake()
		{
			base.OnAwake();
			deckItems = new Dictionary<string, DeckButtonView>();
			deckModels = new Dictionary<string, DeckData>();
			EnumerableExtensions.ForEach(leagueButtons, (button, index) => button.onClick.AddListener(() => OnSelectLeague(index)));
			LobbyBus.RequestRefreshLeagueStatistics.Subscribe(this, () => LoadUserLeagueStatistics().Forget());
			playButton.Subscribe(OnPlayButtonClick);
			backButton.Subscribe(Close);
			InfoButton.Subscribe(OpenLeagueInfo);
			TutorialLeagueMMR.SetHintTarget(TutorialTrigger.LeagueMmr.ToString()).SetTransitionFactorSize().Init();
			TutorialLeagueFlags.SetHintTarget(TutorialTrigger.LeagueFlags.ToString()).SetTransitionFactorSize().Init();
			TutorialLeagueDecks.SetHintTarget(TutorialTrigger.LeagueDecks.ToString()).SetTransitionFactorSize().Init();
		}

		public async UniTask InitAndShowAsync(string leagueId = null, string deckId = null)
		{
			if (!await CheckLeagues().AddLoadingTask() || !await CheckPlayerLeagueStatistics().AddLoadingTask())
				return;

			var releasePervious = updateFlags != null;
			updateFlags?.Cancel();
			updateFlags?.Dispose();
			updateFlags = new CancellationTokenSource();
			deckModels = DeckApplicationAdapter.Application.All.ToDictionary(x => x.Id);
			currentLeagueIndex = Mathf.Max(0, LobbyBus.Leagues.Value.FindIndex(o => o.Id == leagueId));
			
			for (var i = 0; i < leagueButtons.Count; i++)
			{
				var leagueFlag = leagueImages[i];
				var leagueFlagAvailable = i < LobbyBus.Leagues.Value.Count;
				if (leagueFlagAvailable)
				{
					var url = LobbyBus.Leagues.Value[i].GetArtURL();
					leagueImages[i].LoadResourceAsync(url, updateFlags.Token, releasePervious).Forget();
				}
				SetActive(leagueFlag, leagueFlagAvailable);
			}
			
			playerView.SetName(User.UserName);
			playerView.SetArtFromOwnedVulcaniteId(DeckApplicationAdapter.Application.Current.OwnedVulcaniteId);
			LobbyBus.OnUserDataRefreshed.SubscribeRaw(OnRentRefreshed);
			CreateDeckItems();
			OnSelectLeague(currentLeagueIndex);
			selectedDeckValid = ValidateDeck(deckId);
			selectedDeck = deckId;
			SelectDeck();
			Show();

			if (!string.IsNullOrWhiteSpace(leagueId) && !string.IsNullOrWhiteSpace(deckId))
			{
				OnPlayButtonClick();
			}
			
			LobbyLeagueInfoView.Instance.TryToShow();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			deckModels?.Clear();
			deckItems?.Clear();
			decksContainer.DestroyChildren();
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnRentRefreshed);
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnRentRefreshed);
		}
		
		private void SearchSessionAsync()
		{
			LeagueApplicationAdapter.Application.OpenLeagueAsync(LobbyBus.CurrentLeagueId, selectedDeck).AddLoadingTask();
		}
		
		private void ConiformationPopup(string message = null, Action<bool> onResponse = null)
		{
			ConfirmationDialog.Instance.Init()
				.SetMessage(message ?? "Your deck does not qualify for the selected league. Do you want to edit your deck?")
				.SetTitle("Leagues")
				.SetResponse(onResponse)
				.SetCancel(onResponse != null ? "Cancel" : null)
				.Apply();
		}

		private async Task<bool> CheckLeagues()
		{
			if (!LobbyBus.Leagues.Value.IsNullOrEmpty())
				return true;
			
			var leaguesRequest = new TaskCompletionSource<List<LeagueModel>>();
			LobbyBus.Leagues.SubscribeRaw(LeaguesUpdate);
			LobbyBus.LeaguesRefereshRequered.Publish();

			void LeaguesUpdate(List<LeagueModel> leagues)
			{
				LobbyBus.Leagues.Unsubscribe(LeaguesUpdate);
				leaguesRequest.SetResult(leagues);
			}

			return !(await leaguesRequest.Task).IsNullOrEmpty();
		}

		private async Task<bool> CheckPlayerLeagueStatistics()
		{
			if (leagueStatistics == null)
				return await LoadUserLeagueStatistics();

			return leagueStatistics != null;
		}

		private async UniTask<bool> LoadUserLeagueStatistics()
		{
			try
			{
				var responce = await PlayerAPI.GetStatisticsLeague().AddLoadingTask();
				leagueStatistics = responce.Data ?? new List<UserStatisticsLeagueData>();
				return responce;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return false;
			}
		}

		private void CreateDeckItems()
		{
			deckItems.Clear();
			decksContainer.DestroyChildren();

			foreach (var deck in deckModels.Values)
			{
				var flags = LobbyBus.Leagues.Value
					.Where(league => league.IsDeckValid(deck))
					.Select(x => x.GetArtURL())
					.ToArray();

				var ownedHero = VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == deck.OwnedVulcaniteId);
				var heroData = GameDataBaseAdapter.Instance.GetHero(ownedHero?.VulcaniteId);
				var deckItem = Instantiate(deckButtonPrefab, decksContainer);
				
				deck.DeckValue = DeckValueApplicationAdapter.Application
					.CalculateDeckValue(DeckApplicationAdapter.Application.OwnedCards.Where(x=> deck.OwnedCardIds.Contains(x.Id)));
				
				deckItem.Id = deck.Id;
				deckItem.SetDeckName(deck.Name);
				deckItem.SetVulcaniteImage(heroData?.ArtUrl);
				deckItem.SetCoatImage($"{heroData?.Quadrant ?? Quadrant.Neutral}_Flag");
				deckItem.SetCardCount($"{deck.OwnedCardIds.Count}/{SharedConfigAdapter.Config.MaxCardsInDeck}");
				deckItem.SetLeagueFlags(flags);
				deckItem.SetFactionFlag($"{deck.Faction}_Flag");
				deckItem.SetSelectedAction(OnDeckItemClick);
				deckItems.Add(deck.Id, deckItem);
				SetActive(deckItem, true);
			}
			OnRentRefreshed();
		}

		private void OnRentRefreshed()
		{
			if (deckItems == null || deckItems.Count == 0 || deckModels == null || deckModels.Count == 0)
				return;

			foreach (var (deckId, deckItem) in deckItems)
			{
				var deckModel = deckModels[deckId];
				var isDeckValid = deckModel.IsValid();
				deckItem.SetWarning(!isDeckValid);
				
				// only set false if deck is not valid
				// do not this : deckItem.SetInteractable(isDeckValid)
				if (!isDeckValid)
					deckItem.SetInteractable(false);
			}
		}
		
		private bool ValidateDeck(string deckId)
		{
			if (LobbyBus.Leagues?.Value == null)
			{
				RRLogger.Warning($"[{"LeagueView.ValidateDeck".Red().Bold()}] - Leagues is missing");
				return false;
			}

			if (string.IsNullOrWhiteSpace(deckId))
				return false;
			
			if(currentLeagueIndex >= LobbyBus.Leagues.Value.Count || currentLeagueIndex < 0)
				throw new IndexOutOfRangeException($"League with index does not exist : {currentLeagueIndex}");
			
			var league = LobbyBus.Leagues.Value[currentLeagueIndex];
			return league.IsDeckValid(deckModels[deckId]);
		}

		private async void SelectDeck()
		{
			SetInteractable(playButton, false);
			var availableDeck = !string.IsNullOrEmpty(selectedDeck) 
			                    && selectedDeckValid
								&& await DeckApplicationAdapter.Application.SelectAsync(deckModels[selectedDeck]);

			SetInteractable(playButton, availableDeck);
			UpdateItemsSelection();
		}

		private void UpdateItem(DeckButtonView deckItem)
		{
			var isValid = ValidateDeck(deckItem.Id);

			if (string.IsNullOrEmpty(selectedDeck))
				selectedDeckValid = false;
			
			if (selectedDeck == deckItem.Id)
				selectedDeckValid = isValid;
			
			deckItem.SetInteractable(isValid);
		}
		
		private void UpdateItemsSelection()
		{
			EnumerableExtensions.ForEach(deckItems.Values, x=> x.SetSelect(x.Id == selectedDeck));
		}
		
		private void OnDeckItemClick(string id)
		{
			if(!deckModels.ContainsKey(id))
				return;
			
			selectedDeck = id;
			selectedDeckValid = ValidateDeck(id);
			SelectDeck();
		}
		
		private void OnSelectLeague(int leagueIndex)
		{
			EnumerableExtensions.ForEach(leagueSelectionObjects, (x, i) => x.SetActive(i == leagueIndex));

			currentLeagueIndex = leagueIndex;
			var currentLeague = LobbyBus.Leagues.Value[currentLeagueIndex];
			
			Set(leagueNameText, currentLeague.LeagueName);
			

			var elo = leagueStatistics?.FirstOrDefault(m => m.LeagueId == currentLeague.Id)?.ELO ?? DEFAULT_MMR;
			MMRText.SetText($"MMR: {elo}");
			if (currentLeague.LeagueName == "Ranked")
			{
				Set(leagueDescriptionText, currentLeague.GetDescription());
			}
			else
			{
				Set(leagueDescriptionText, "");
			}

			Debug.Log($"[League Description] {currentLeague.GetDescription()}");
			
			
			EnumerableExtensions.ForEach(deckItems.Values, UpdateItem);
			selectedDeck = null;
			selectedDeckValid = false;
			SelectDeck();
			
			LobbyBus.CurrentLeagueId.Publish(currentLeague.Id);
		}
		
		private void OnPlayButtonClick()
		{
			if (string.IsNullOrEmpty(selectedDeck) || !selectedDeckValid)
			{
				ConiformationPopup(onResponse: TrySwitchToDeckBuilder);
				return;
			}
			
			SearchSessionAsync();
		}

		private void TrySwitchToDeckBuilder(bool userResponse)
		{
			if (!userResponse)
				return;
					
			PlayerDecksView.Instance.InitAndShow();
			Close();
		}

		private void OnDestroy()
		{
			
			if (updateFlags != null)
			{
				updateFlags.Cancel();
				updateFlags.Dispose();
				
				if (leagueImages != null) 
					foreach (var rawImage in leagueImages.Where(x => x.IsActive())) 
						rawImage.ReleaseResource();
			}
			
			leagueStatistics = null;
			deckModels?.Clear();
			deckItems?.Clear();
			decksContainer.DestroyChildren();
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnRentRefreshed);
			updateFlags = null;
		}

		private void OpenLeagueInfo()
		{
			LobbyLeagueInfoView.Instance.Show();
		}
	}
}