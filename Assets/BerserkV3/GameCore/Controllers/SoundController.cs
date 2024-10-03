using Audio;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.Controllers.Graveyard;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using Vulcan.Audio;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public class SoundController : DisposableWithCts, IInitializable
	{
		private readonly IHoveringSystem hoveringSystem;
		private readonly IGameContext gameContext;
		private readonly IDragDropSystem dragDropSystem;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IPreviewSystem previewSystem;
		private readonly ISelectionSystem selectionSystem;
		private readonly IManualArrowSystem manualArrowSystem;

		public SoundController(IHoveringSystem hoveringSystem, 
			IGameContext gameContext,
			IDragDropSystem dragDropSystem, 
			IGameLogicEventsSource gameLogicEventsSource,
			IPreviewSystem previewSystem,
			ISelectionSystem selectionSystem,
			IManualArrowSystem manualArrowSystem)
		{
			this.hoveringSystem = hoveringSystem;
			this.gameContext = gameContext;
			this.dragDropSystem = dragDropSystem;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.previewSystem = previewSystem;
			this.selectionSystem = selectionSystem;
			this.manualArrowSystem = manualArrowSystem;
		}

		public void Initialize()
		{
			hoveringSystem.OnHoverEnter += OnHoverEnter;
			dragDropSystem.OnDragCanceled += OnDragCancelled;
			dragDropSystem.OnDragIn += OnDropIn;
			previewSystem.OnPreview += OnPreview;
			selectionSystem.OnSelected += OnSelected;
			manualArrowSystem.OnArrowActivityChanged += OnArrowActivityChanged;
			gameLogicEventsSource.Subscribe<ChangeCardsState>(OnCardsStateChanged, Token);
		}

		private void OnArrowActivityChanged()
		{
			if (manualArrowSystem.IsActive)
				AudioController.Play(Clip.Arrow_Start);
		}

		public override void Dispose()
		{
			base.Dispose();
			hoveringSystem.OnHoverEnter -= OnHoverEnter;
			dragDropSystem.OnDragCanceled -= OnDragCancelled;
			dragDropSystem.OnDragIn -= OnDropIn;
			previewSystem.OnPreview -= OnPreview;
			selectionSystem.OnSelected -= OnSelected;
			manualArrowSystem.OnArrowActivityChanged -= OnArrowActivityChanged;
		}

		private void OnCardsStateChanged(ChangeCardsState data)
		{
			if (data.NewState == RuntimeState.InHand || data.OldState == RuntimeState.InHand)
				AudioController.Play(Clip.Card_Spawn); // its rearrange clip, naming wrong!
		}

		private void OnDropIn(IDraggable draggable, IDropHandler dropHandler)
		{
			if (draggable?.TargetView == null || !draggable.TargetView.TryGetComponent(out ICardView cardView))
				return;
			
			AudioController.Play(cardView.RuntimeGameObject.Data.Type == ObjectType.Spell
				? Clip.Spell_Release
				: Clip.Card_Release);
		}

		private void OnDragCancelled(IDraggable draggable, IDropHandler dropHandler)
		{
			if (draggable?.TargetView == null || !draggable.TargetView.TryGetComponent(out ICardView cardView))
				return;
			
			AudioController.Play(cardView.RuntimeGameObject.Data.Type == ObjectType.Spell
				? Clip.Spell_Release
				: Clip.Card_Release);
		}

		private void OnHoverEnter(IHoverable hoverable)
		{
			if (hoverable is ICardStrategy)
			{
				if (manualArrowSystem.IsActive)
				{
					AudioController.Play(Clip.Arrow_Select);
					return;
				}
				AudioController.Play(Clip.Card_Hover);
			}
		}

		private void OnPreview()
		{
			if (previewSystem.Current is not ICardStrategy)
				AudioController.Play(Clip.Card_Hover);
		}
		
		private void OnSelected(ISelectable selectable)
		{
			if (gameContext.Timer.RuntimeData == null) 
				return;
			
			switch (gameContext.Timer.RuntimeData.State)
			{
				case TimerState.Mulligan:
					if (selectable is ICardStrategy)
						AudioController.Play(Clip.Arrow_Select);
					break;
					
				case TimerState.Game:
					if (selectable is IGraveyardPileController)
						AudioController.Play(Clip.Arrow_Select);
					
					if (selectable is ICardStrategy && manualArrowSystem.IsActive)
						AudioController.Play(Clip.Arrow_Select);
					break;
			}
		}
	}
}