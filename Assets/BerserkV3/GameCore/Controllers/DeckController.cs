using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Customisations;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.UI;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public class DeckController : DisposableWithCts, IInitializable, IPreviewable
	{
		private readonly IGameCustomisationApplication gameCustomisationApplication;
		private readonly IManualArrowSystem manualArrowSystem;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameRepository gameRepository;
		private readonly IPreviewSystem previewSystem;
		private readonly IDragDropSystem dragDropSystem;
		private readonly IList<IDeckHolderView> views;
		private readonly ISharedConfig sharedConfig;

		[Inject]
		public DeckController(IEnumerable<IDeckHolderView> holderViews,
			IGameCustomisationApplication gameCustomisationApplication,
			IManualArrowSystem manualArrowSystem,
			IGameLogicEventsSource gameLogicEventsSource,
			IGameRepository gameRepository,
			IPreviewSystem previewSystem,
			IDragDropSystem dragDropSystem,
			ISharedConfig sharedConfig)
		{
			this.gameCustomisationApplication = gameCustomisationApplication;
			this.manualArrowSystem = manualArrowSystem;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameRepository = gameRepository;
			this.previewSystem = previewSystem;
			this.dragDropSystem = dragDropSystem;
			this.sharedConfig = sharedConfig;


			views = new List<IDeckHolderView>(holderViews);
			PreviewSettings = new PreviewSettings(PreviewType.Pile);
		}

		public void Initialize()
		{
			gameCustomisationApplication.SubscribeOnReady(LoadCustomisations);
			gameLogicEventsSource.Subscribe<ChangeDeckCount>(UpdateViews, Token);
			previewSystem.Registration(this);
		}

		public override void Dispose()
		{
			base.Dispose();
			previewSystem.UnRegistration(this);
		}

		private UniTask LoadCustomisations()
		{
			var selfAssetData = gameCustomisationApplication
				.Get<AssetData>(gameRepository.SelfId, CustomisationType.CardBack);
			
			var opponentAssetData = gameCustomisationApplication
				.Get<AssetData>(gameRepository.OpponentId, CustomisationType.CardBack);
			
			var selfView = GetViewByOwner(Owner.Self);
			var opponentView = GetViewByOwner(Owner.Opponent);

			return UniTask.WhenAll(
				selfView.SetArt(selfAssetData.URL, Token),
				opponentView.SetArt(opponentAssetData.URL, Token));
		}

		private void UpdateViews(ChangeDeckCount data)
		{
			var owner = gameRepository.GetOwnerByUserId(data.UserId);
			var view = GetViewByOwner(owner);
			view?.SetCountText(data.DeckCount.ToString());
		}
		
		private IDeckHolderView GetViewByOwner(Owner owner)
		{
			return views.FirstOrDefault(x => x.Owner == owner);
		}

	#region Previewable
		public GameObject TargetView => GetViewByOwner(Owner.Self).TargetView;
		public IPreviewData PreviewData
		{
			get
			{
				var nextCard = gameRepository.NextDeckCard;
				if (nextCard == null)
					return null;

				var preview = new PreviewCardData(nextCard);
				ApplyOffFactionLava(preview);
				return preview;
			}
		}
		public IPreviewSetting PreviewSettings { get; }
		public bool CanPreview()
		{
			return !manualArrowSystem.IsActive && !dragDropSystem.AnyDragged && PreviewData != null;
		}
	#endregion
	
	private void ApplyOffFactionLava(PreviewCardData preview)
	{
		if (preview == null)
			return;

		var offFactionConfig = sharedConfig.OffFactionLavaConfig;
		if (offFactionConfig == null || !offFactionConfig.Enabled)
			return;

		var selfHero = gameRepository.GetHeroByUserId(gameRepository.SelfId);
		if (selfHero == null)
			return;

		var heroQuadrant = selfHero.RuntimeGameObject.Data.Quadrant;
		var cardQuadrant = preview.Quadrant;
		var baseMana = preview.Mana;
		var maxLava = sharedConfig.PlayerMaxMana;

		var isNeutral =
			offFactionConfig.NeutralQuadrants != null &&
			offFactionConfig.NeutralQuadrants.Contains(cardQuadrant);

		var effectiveMana = baseMana;

		if (!isNeutral && cardQuadrant != heroQuadrant)
			effectiveMana = baseMana + offFactionConfig.PenaltyPerCard;

		if (effectiveMana > maxLava)
			effectiveMana = maxLava;

		preview.SetPreviewMana(effectiveMana);
	}
	}
}