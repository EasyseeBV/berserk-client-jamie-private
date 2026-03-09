using System;
using System.Linq;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.RuntimeObjects;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
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

namespace BerserkV3.GameCore.Controllers.Graveyard
{
	public class GraveyardPileController : DisposableWithCts, IGraveyardPileController, IPreviewable, ISelectable
	{
		private readonly IGameCustomisationApplication gameCustomisationApplication;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGraveyardPileView graveyardPileView;
		private readonly IManualArrowSystem manualArrowSystem;
		private readonly ISelectionSystem selectionSystem;
		private readonly IDragDropSystem dragDropSystem;
		private readonly IGameRepository gameRepository;
		private readonly IPreviewSystem previewSystem;
		private readonly IGraveyardView graveyardView;
		private readonly IGameContext gameContext;

		public Owner Owner { get; }

		public GraveyardPileController(
			IGameCustomisationApplication gameCustomisationApplication,
			IGameLogicEventsSource gameLogicEventsSource,
			IGraveyardPileView graveyardPileView,
			IManualArrowSystem manualArrowSystem,
			ISelectionSystem selectionSystem,
			IDragDropSystem dragDropSystem,
			IGameRepository gameRepository,
			IPreviewSystem previewSystem,
			IGraveyardView graveyardView,
			IGameContext gameContext)
		{
			this.gameCustomisationApplication = gameCustomisationApplication;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.graveyardPileView = graveyardPileView;
			this.manualArrowSystem = manualArrowSystem;
			this.selectionSystem = selectionSystem;
			this.dragDropSystem = dragDropSystem;
			this.gameRepository = gameRepository;
			this.previewSystem = previewSystem;
			this.graveyardView = graveyardView;
			this.gameContext = gameContext;
			Owner = graveyardPileView.Owner;
			PreviewSettings = new PreviewSettings(PreviewType.Pile);
		}

		public void Initialize()
		{
			gameCustomisationApplication.SubscribeOnReady(LoadCustomisations);
			gameLogicEventsSource.Subscribe<ChangeCardsState>(UpdateView, Token, int.MaxValue);
			gameLogicEventsSource.Subscribe<InitializeGame>(UpdateView, Token, int.MaxValue);
			selectionSystem.OnSelected += OnSelected;
			selectionSystem.Registration(this);
			previewSystem.Registration(this);
		}

		private UniTask LoadCustomisations()
		{
			var assetData = gameCustomisationApplication
				.Get<AssetData>(gameRepository.GetUserIdByOwner(Owner), CustomisationType.CardBack);
			
			return graveyardPileView.SetArt(assetData.URL, Token);
		}

		public override void Dispose()
		{
			base.Dispose();
			selectionSystem.OnSelected -= OnSelected;
			selectionSystem.UnRegistration(this);
			previewSystem.UnRegistration(this);
		}

		private void UpdateView()
		{
			var cardCount = gameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InDiscard, gameRepository.GetUserIdByOwner(Owner), asQuery: true)
				.Count();
			graveyardPileView.UpdatePile(cardCount);
		}

	#region Previewable
		GameObject IPreviewable.TargetView => graveyardPileView.TargetView;

		public IPreviewData PreviewData
		{
			get
			{
				var topCard = gameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InDiscard, gameRepository.GetUserIdByOwner(Owner), asQuery: true)
					.OrderByDescending(x => x.RuntimeData.RelativePositionX)
					.FirstOrDefault();

				if (topCard == null)
					return null;

				return new PreviewCardData(topCard.RuntimeData, topCard.Data);
			}
		}

		public IPreviewSetting PreviewSettings { get; }

		public bool CanPreview()
		{
			return CanSelect() && PreviewData != null;
		}
	#endregion

	#region Selectable
		GameObject ISelectable.TargetView => graveyardPileView.TargetView;

		public bool CanSelect()
		{
			return !manualArrowSystem.IsActive && !dragDropSystem.AnyDragged && !graveyardView.IsShowed;
		}

		private void OnSelected(ISelectable selectable)
		{
			if (selectable != this)
				return;
			
			previewSystem.Close();
		}

	#endregion
	}
}