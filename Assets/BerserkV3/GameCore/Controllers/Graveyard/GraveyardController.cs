using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.UI;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Controllers.Graveyard
{
	public class GraveyardController : DisposableWithCts, IInitializable
	{
		private readonly List<IGraveyardPileController> pileControllers;
		private readonly IEnumerable<IGraveyardPileView> graveyardPileViews;
		private readonly IManualArrowSystem manualArrowSystem;
		private readonly ISelectionSystem selectionSystem;
		private readonly IDragDropSystem dragDropSystem;
		private readonly IPreviewSystem previewSystem;
		private readonly IGraveyardView graveyardView;
		private readonly IInstantiator instantiator;

		public GraveyardController(
			IEnumerable<IGraveyardPileView> graveyardPileViews,
			IManualArrowSystem manualArrowSystem,
			ISelectionSystem selectionSystem,
			IDragDropSystem dragDropSystem,
			IPreviewSystem previewSystem,
			IGraveyardView graveyardView,
			IInstantiator instantiator)
		{
			this.graveyardPileViews = graveyardPileViews;
			this.manualArrowSystem = manualArrowSystem;
			this.selectionSystem = selectionSystem;
			this.dragDropSystem = dragDropSystem;
			this.previewSystem = previewSystem;
			this.graveyardView = graveyardView;
			this.instantiator = instantiator;
			pileControllers = new List<IGraveyardPileController>();
		}

		public void Initialize()
		{
			foreach (var graveyardPileView in graveyardPileViews)
			{
				var controller = instantiator.Instantiate<GraveyardPileController>(new object[] { graveyardPileView });
				controller.Initialize();
				pileControllers.Add(controller);
			}

			manualArrowSystem.OnArrowActivityChanged += TryCloseGraveyard;
			graveyardView.OnCloseClick += TryCloseGraveyard;
			selectionSystem.OnSelected += TryOpenGaveyard;
		}

		public override void Dispose()
		{
			base.Dispose();
			manualArrowSystem.OnArrowActivityChanged -= TryCloseGraveyard;
			graveyardView.OnCloseClick -= TryCloseGraveyard;
			selectionSystem.OnSelected -= TryOpenGaveyard;
			pileControllers.ForEach(x => x.Dispose());
			pileControllers.Clear();
		}

		private void TryCloseGraveyard()
		{
			if (!graveyardView.IsShowed)
				return;

			graveyardView.Close();
		}

		private void TryOpenGaveyard(ISelectable selectable)
		{
			if (graveyardView.IsShowed
				|| manualArrowSystem.IsActive
				|| dragDropSystem.AnyDragged
				|| selectable is not IGraveyardPileController deckPile)
				return;

			if (Application.isMobilePlatform && previewSystem.Current is IGraveyardPileController)
				return;
			
			graveyardView.OpponentContainer.gameObject.SetActive(deckPile.Owner == Owner.Opponent);
			graveyardView.SelfContainer.gameObject.SetActive(deckPile.Owner == Owner.Self);
			graveyardView.Show();
		}
	}
}