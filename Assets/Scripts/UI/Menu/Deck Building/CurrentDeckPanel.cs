using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.UI;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using System;

namespace UI
{
	public partial class CurrentDeckPanel : BaseView
	{
		public DeckCardCollection CardCollection { get; private set; }
		public IDeckValue DeckValue { get; private set; }
		
		private readonly Dictionary<string, int> baseManaByCardId = new();
		private readonly Dictionary<(string cardId, int heroQuadrant), int> adjustedManaCache = new();

		public string DeckName
		{
			get => deckNameInput.text;
			set => deckNameInput.text = value;
		}

		public OwnedVulcanite OwnedHero
		{
			get => ownedHero;
			set
			{
				ownedHero = value;
				heroData = GameDataBaseAdapter.Instance.GetHero(ownedHero.VulcaniteId);
				VulcaniteImage.LoadResourceAsync(heroData?.ArtUrl).Forget();
			}
		}

		[SerializeField] private TextInputField deckNameInput;
		[SerializeField] private bool allowDrop;

		private List<ProceduralImage> lavaColumns;
		private LoopScrollRefresher loopScrollRefresher;
		private OwnedVulcanite ownedHero;
		private HeroData heroData;


		protected override void OnAwake()
		{
			ProfilePictureButton.onClick.AddListener(VulcaniteSelectionShow);
			ProfilePictureButton.SetHintTarget(TutorialTrigger.DeckVulcaniteChange.ToString()).SetTransitionFactorSize().Init();
			DeckValue = new DeckValueController(DeckValueView, PreviewSystemAdapter.Instance, DeckValueApplicationAdapter.Application);
		}

		private void OnDestroy()
		{
			DeckValue?.Dispose();
			DeckValue = null;
		}

		public override bool CanDropIn(BaseView draggedView)
		{
			return allowDrop
			       && draggedView is DeckBuilderCardView view
			       && CardCollection.CanAdd(view.CardData);
		}

		private void VulcaniteSelectionShow()
		{
			DeckSelectVulcaniteDialog.Instance.InitAndShow(ownedHero.VulcaniteId, SelectVulcanite);
		}

		private void SelectVulcanite(OwnedVulcanite value)
		{
			OwnedHero = value;
		}

		public void SetUp(DeckCardCollection cardCollection, bool isNewDeck = false)
		{
			loopScrollRefresher = new LoopScrollRefresher(LoopVerticalScrollRect);

			if (isNewDeck)
				VulcaniteSelectionShow();

			CardCollection = cardCollection;
			CardCollection.OnStackAdded += RefreshPanel;
			CardCollection.OnStackRemoved += RefreshPanel;
			CardCollection.OnCollectionChanged += RefreshCount;

			if (!gameObject.activeSelf)
				return;

			RefreshPanel();
			RefreshCount();
		}

		private void RefreshCount()
		{
			Set(CardsCountText, $"{CardCollection.FullCount}/{CardCollection.Limit}");
			var deckCards = CardCollection.GetAll();
			DeckValue.Refresh(deckCards.SelectMany(x => x.GetAll()));
			HistogramView.Refresh(deckCards.SelectMany(x => Enumerable.Repeat(x.CardData, x.Count)).ToArray());
		}

		private void RefreshPanel(IDeckCardStack deckCardStack = null)
		{
			var deckCards = CardCollection.ToFiltered();

			var heroQuadrant = heroData.Quadrant;

			foreach (var stack in deckCards)
			{
				var cardData = stack.CardData;
				
				if (cardData.Quadrant == heroQuadrant)
					continue;
				
				if (!baseManaByCardId.TryGetValue(cardData.Id, out var baseMana))
				{
					baseMana = cardData.Mana;
					baseManaByCardId[cardData.Id] = baseMana;
				}
				
				var cacheKey = (cardData.Id, (int)heroQuadrant);
				if (!adjustedManaCache.TryGetValue(cacheKey, out var adjustedMana))
				{
					adjustedMana = CalculateAdjustedCost(baseMana);
					adjustedManaCache[cacheKey] = adjustedMana;
				}
				
				cardData.Mana = adjustedMana;
			}

			loopScrollRefresher.ScrollToCard(deckCards, deckCardStack);
			SetActive(InfoText, CardCollection.Count == 0);
		}

		
		public static int CalculateAdjustedCost(int baseLava)
		{
			if (baseLava <= 0)
				return 1;
			
			double logValue = Math.Log(baseLava + 1, 2);
			int increase = (int)Math.Ceiling(logValue);
    
			int adjustedLava = baseLava + increase;
    
			return Math.Min(adjustedLava, 10);
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			VulcaniteImage.ReleaseResource();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			loopScrollRefresher?.Release();
		}
	}
}