using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.UI;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using Zenject;

namespace UI
{
	public partial class CurrentDeckPanel : BaseView
	{
		[Inject]
		private ISharedConfig sharedConfig;
		public DeckCardCollection CardCollection { get; private set; }
		public IDeckValue DeckValue { get; private set; }

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
			
			var items = GetComponentsInChildren<DeckPanelItemView>(true);
			foreach (var item in items)
			{
				item.ForceOffFactionRefresh();
			}
			RefreshCount();
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
			
			var histogramCosts = deckCards
				.SelectMany(x => Enumerable.Repeat(x.CardData, x.Count))
				.Select(cardData => CalculateOffFactionLavaForCard(cardData))
				.ToArray();

			HistogramView.Refresh(histogramCosts);
		}


		private void RefreshPanel(IDeckCardStack deckCardStack = null)
		{
			var deckCards = CardCollection.ToFiltered();
			loopScrollRefresher.ScrollToCard(deckCards, deckCardStack);
			SetActive(InfoText, CardCollection.Count == 0);
		}

		public int CalculateOffFactionLavaForCard(ICardData cardData)
		{
			var offFactionConfig = sharedConfig.OffFactionLavaConfig;

			if (offFactionConfig == null)
			{
				RRLogger.Log("[OffFactionLava][DeckPanel] Config is NULL, return base mana");
				return cardData.Mana;
			}

			if (!offFactionConfig.Enabled)
			{
				RRLogger.Log("[OffFactionLava][DeckPanel] Feature DISABLED in config, return base mana");
				return cardData.Mana;
			}

			if (heroData == null)
			{
				RRLogger.Log("[OffFactionLava][DeckPanel] heroData is NULL (vulcanite not selected yet?), return base mana");
				return cardData.Mana;
			}

			var heroQuadrant = heroData.Quadrant;
			var cardQuadrant = cardData.Quadrant;
			var baseLava = cardData.Mana;
			var maxLava = sharedConfig.PlayerMaxMana;

			var isNeutral =
				offFactionConfig.NeutralQuadrants != null &&
				offFactionConfig.NeutralQuadrants.Contains(cardQuadrant);

			RRLogger.Log(
				$"[OffFactionLava][DeckPanel] Card='{cardData.Title}', HeroQ={heroQuadrant}, CardQ={cardQuadrant}, Base={baseLava}, IsNeutral={isNeutral}");

			var effectiveLava = baseLava;

			if (!isNeutral && cardQuadrant != heroQuadrant)
			{
				effectiveLava = baseLava + offFactionConfig.PenaltyPerCard;
				RRLogger.Log(
					$"[OffFactionLava][DeckPanel] OFF-FACTION: +{offFactionConfig.PenaltyPerCard} → {effectiveLava}");
			}
			else
			{
				RRLogger.Log("[OffFactionLava][DeckPanel] No penalty applied");
			}

			if (effectiveLava > maxLava)
			{
				RRLogger.Log(
					$"[OffFactionLava][DeckPanel] Clamp {effectiveLava} to maxLava {maxLava}");
				effectiveLava = maxLava;
			}

			RRLogger.Log(
				$"[OffFactionLava][DeckPanel] RESULT: Card='{cardData.Title}', Base={baseLava}, Effective={effectiveLava}");

			return effectiveLava;
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