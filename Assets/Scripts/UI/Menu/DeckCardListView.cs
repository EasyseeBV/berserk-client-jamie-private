using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.LiveLinkRouter;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Deck;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public partial class DeckCardListView : BaseView
	{
		[SerializeField] private RectTransform SortPanel;
		[SerializeField] private List<Toggle> sortButtons;
		private readonly List<IDisposable> disposables = new();
		private SortingType lastSelected;
		private DeckData currentDeck;
		private CanvasGroup canvasGroup;
		private bool hasChanges;
		
		protected override void OnAwake()
		{
			TryGetComponent(out canvasGroup);
			SearchInput.OnFilterChanged += ApplySearchFilter;

			BuyCardsButton.Subscribe(RedirectToMarket);
			BackButton.Subscribe(TryClose);
			SaveButton.Subscribe(SaveDeck);
			SortButton.Subscribe(() => SortPanel.gameObject.SetActive(!SortPanel.gameObject.activeSelf));

			foreach (SortingType sortingType in Enum.GetValues(typeof(SortingType)))
			{
				var toggle = sortButtons[(int) sortingType];
				toggle.onValueChanged.AddListener(flag =>
				{
					if (flag) 
						ApplySorting(sortingType);
				});
				if (toggle.isOn)
					lastSelected = sortingType;
			}

			TutorialSearchRect.SetHintTarget(TutorialTrigger.DeckEditMenuExit.ToString()).SetTransitionFactorSize().Init();
			TutorialCurrentCards.SetHintTarget(TutorialTrigger.DeckCurrentCards.ToString()).SetTransitionFactorSize().Init();
			TutorialAvailableCards.SetHintTarget(TutorialTrigger.DeckAvailableCards.ToString()).SetTransitionFactorSize().Init();
		}

		private void RedirectToMarket()
		{
			LiveLinkRouterAdapter.Service.OpenLinkByKey(LinkKeyHelper.MARKET);
		}

		public void InitAndShow(DeckData deckModel)
		{
			SetActive(this, true);
			canvasGroup.alpha = 0;
			var isNewDeck = string.IsNullOrEmpty(deckModel.Id);
			InitAsync().AddLoadingTask().Forget();

			async UniTask InitAsync()
			{
				await UniTask.Yield();
				currentDeck = deckModel;
				DeckCardListPanel.DeckName = currentDeck?.Name;
				DeckCardListPanel.OwnedHero = VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == currentDeck.OwnedVulcaniteId);
				hasChanges = false;
				DisplayCards(isNewDeck);
				await UniTask.Yield();
				canvasGroup.alpha = 1;
				Show(noAnimation: true);
				LobbyBus.OnDeckEditEntered += true;
			}
		}

		private void ApplySorting(SortingType sortingType)
		{
			if (AllCardsCopyPanel.CardCollection == null)
				return;

			lastSelected = sortingType;
			AllCardsCopyPanel.CardCollection.ApplySortingFunction(sortingType);
			
			SetActive(SortPanel, false);
		}

		private void ApplySearchFilter()
		{
			AllCardsCopyPanel.CardCollection?.ApplyFilterFunction(SearchInput.FilterFunction);
		}

		private async void SaveDeck()
		{
			if (!ValidateCurrentDeck())
				return;
			
			currentDeck.OwnedVulcaniteId = DeckCardListPanel.OwnedHero.Id;
			currentDeck.Name = DeckCardListPanel.DeckName;
			currentDeck.DeckValue = DeckCardListPanel.DeckValue.Value;
			currentDeck.OwnedCardIds = DeckCardListPanel.CardCollection
				.ToOrdered()
				.SelectMany(stack => stack.GetAll().Select(x=> x.Id))
				.ToList();
			
			var notOwnedCards = User.OwnedCards
				.Where(x => !x.IsOwned)
				.ToList();
			
			var notOwnedInDeck = notOwnedCards
				.Where(c => currentDeck.OwnedCardIds.Contains(c.Id))
				.ToList();
			RRLogger.Error($"Not Owned Cards in current deck: {string.Join(", ", notOwnedInDeck.Select(c => c.CardId))}");

			if (!await DeckApplicationAdapter.Application.SaveAsync(currentDeck)) 
				return;

			await DeckApplicationAdapter.Application.SelectAsync(currentDeck);
			
			hasChanges = false;
			LobbyBus.OnDeckSaved += true;
			TryClose();
		}

		private bool ValidateCurrentDeck()
		{
			string message;
			var ownedHeroId = DeckCardListPanel.OwnedHero?.Id;
			var ownedVulcanite = VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == ownedHeroId);
			if (ownedVulcanite == null)
			{
				message = GameDataBaseAdapter.Instance.GetLocalization("DeckChooseVulcanite");
			}
			else if(!DeckCardListPanel.CardCollection.IsValid())
			{
				message = GameDataBaseAdapter.Instance.GetLocalization("DeckUnavailableCards");
			}
			else if(!ownedVulcanite.IsValid())
			{
				message = GameDataBaseAdapter.Instance.GetLocalization("DeckUnavailableVulcanite");
			}
			else if (string.IsNullOrEmpty(DeckCardListPanel.DeckName))
			{
				message = GameDataBaseAdapter.Instance.GetLocalization("DeckName");
			}
			else if (DeckCardListPanel.CardCollection.FullCount < SharedConfigAdapter.Config.MinCardsInDeck)
			{
				message = string.Format(GameDataBaseAdapter.Instance.GetLocalization("DeckMinCards"), SharedConfigAdapter.Config.MinCardsInDeck);
			}
			else if (DeckCardListPanel.CardCollection.FullCount > SharedConfigAdapter.Config.MaxCardsInDeck)
			{
				message = string.Format(GameDataBaseAdapter.Instance.GetLocalization("DeckMaxCards"), SharedConfigAdapter.Config.MaxCardsInDeck);
			}
			else
			{
				return true;
			}
			
			ConfirmationDialog.Instance.Init()
				.SetMessage(message)
				.SetTitle("Information")
				.SetCancel()
				.Apply();
			return false;
		}

		private void TryClose()
		{
			SetActive(SortPanel, false);

			if (!hasChanges)
			{
				CloseInternal();
				return;
			}
			
			ConfirmationDialog.Instance.Init()
				.SetMessage(GameDataBaseAdapter.Instance.GetLocalization("DeckUnsaved"))
				.SetResponseOk(CloseInternal)
				.Apply();
		}

		private void CloseInternal()
		{
			Close();
			PlayerDecksView.Instance.InitAndShow();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			PreviewSystemAdapter.Instance.Close();
			disposables?.ForEach(x=> x?.Dispose());
			disposables?.Clear();
		}
		
		private void DisplayCards(bool isNewDeck = false)
		{
			var ownedCards = new List<OwnedCard>(DeckApplicationAdapter.Application.OwnedCards);
			var deckCardEntityIds = currentDeck?.OwnedCardIds.ToArray() ?? Array.Empty<string>();
			var deckOwnedCards = new List<OwnedCard>();
			// Don't get fooled by Linq, simplifying will break behavior.
			foreach (var deckCardEntityId in deckCardEntityIds)
			{
				var deckCard = ownedCards.FirstOrDefault(x => x.Id == deckCardEntityId);
				if (ownedCards.Remove(deckCard))
					deckOwnedCards.Add(deckCard);
			}

			var deckCollection = new DeckCardCollection(deckOwnedCards, SharedConfigAdapter.Config.MaxCardsInDeck);
			var allCardsCopyCollection = new DeckCardCollection(ownedCards);

			deckCollection.ApplySortingFunction(SortingType.Lava);
			allCardsCopyCollection.ApplySortingFunction(lastSelected);
			allCardsCopyCollection.ApplyFilterFunction(SearchInput.FilterFunction);

			deckCollection.OnCollectionChanged += () => hasChanges = true;
			allCardsCopyCollection.OnCollectionChanged += () => hasChanges = true;
			
			deckCollection.OnCardRemoved += ownedCard => allCardsCopyCollection.AddCard(ownedCard);
			allCardsCopyCollection.OnCardRemoved += ownedCard => deckCollection.AddCard(ownedCard);

			AllCardsCopyPanel.SetUp(allCardsCopyCollection);
			DeckCardListPanel.SetUp(deckCollection, isNewDeck);
			SetActive(SortPanel, false);
			disposables.Add(deckCollection);
			disposables.Add(allCardsCopyCollection);
		}

		private void OnDestroy()
		{
			BuyCardsButton.onClick.RemoveAllListeners();
			BackButton.UnSubscribeAll();
			SaveButton.UnSubscribeAll();
			SortButton.UnSubscribeAll();

			foreach (var sortButton in sortButtons)
				sortButton.onValueChanged.RemoveAllListeners();
			
			SearchInput.OnFilterChanged -= ApplySearchFilter;
			disposables?.ForEach(x=> x?.Dispose());
			disposables?.Clear();
		}
	}

	public enum SortingType
	{
		Quadrant = 0,
		Lava = 1,
		Rarity = 2
	}
}