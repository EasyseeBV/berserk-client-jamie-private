using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Common.InputSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.UI;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.TargetSystem
{
	public class ManualArrowSystem : DisposableWithCts, IManualArrowSystem, IInitializable, ILateTickable
	{
		private readonly IGameLogicContext gameLogicContext;
		private readonly ITargetPickerView targetPickerView;
		private readonly ISelectionSystem selectionSystem;
		private readonly IInputAction pointer;
		private readonly Camera gameCamera;

		private TaskCompletionSource<IList<IRuntimeObjectView>> selectionTask;

		public PickInfo Current { get; private set; }
		
		public bool IsActive => targetPickerView.Enabled && Current != null && selectionTask != null;
		public event Action OnArrowActivityChanged;

		public ManualArrowSystem(
			ITargetPickerView targetPickerView,
			IInputController<TargetSystemActions> inputController,
			IGameLogicContext gameLogicContext,
			ISelectionSystem selectionSystem,
			Camera gameCamera)
		{
			this.targetPickerView = targetPickerView;
			this.gameLogicContext = gameLogicContext;
			this.selectionSystem = selectionSystem;
			this.gameCamera = gameCamera;
			pointer = inputController.Get(TargetSystemActions.Pointer);
		}

		public void Initialize()
		{
			selectionSystem.OnSelected += OnSelected;
			selectionSystem.OnCanceled += OnDeselected;
		}

		public override void Dispose()
		{
			base.Dispose();
			selectionSystem.OnSelected -= OnSelected;
			selectionSystem.OnCanceled -= OnDeselected;
		}

		public Task<IList<IRuntimeObjectView>> GetTargetsAsync(PickInfo pickInfo)
		{
			if (Current != null)
				Cancel();

			if (pickInfo == null)
			{
				Cancel();
				return Task.FromCanceled<IList<IRuntimeObjectView>>(new CancellationToken(true));
			}

			Current = pickInfo;
			selectionTask = new TaskCompletionSource<IList<IRuntimeObjectView>>();
			targetPickerView.Enable(pickInfo.From.SelfContainer);
			targetPickerView.Drag(GetDragPosition());
			OnArrowActivityChanged?.Invoke();

			return selectionTask.Task;
		}

		public bool IsAllowedTarget(IRuntimeObjectView target)
		{
			return IsActive
			       && target != null
			       && Current.From != null
			       && gameLogicContext.TargetConditionRepository
				       .IsAllowedTarget(Current.From.RuntimeGameObject, target.RuntimeGameObject, Current.EffectId, target.RuntimeGameObject);
		}

		public void Cancel()
		{
			selectionTask?.TrySetCanceled();
			CleanupSystem(IsActive);
		}

		public void LateTick()
		{
			if (!IsActive)
				return;

			targetPickerView.Drag(GetDragPosition());
		}

		private void SelectTarget(bool select, params IRuntimeObjectView[] targets)
		{
			if (targets.IsNullOrEmpty())
				return;
			
			foreach (var runtimeLayout in targets.OfType<IRuntimeLayout>())
				runtimeLayout.GlowView?.Enable(select, GlowType.Selection);
		}
		
		private void CleanupSystem(bool notify = true)
		{
			SelectTarget(false, Current?.Targets?.ToArray());
			Current?.Reset();
			Current = null;
			selectionTask = null;
			targetPickerView?.Disable();
			
			if (notify)
				OnArrowActivityChanged?.Invoke();
		}

		private void OnSelected(ISelectable selectable)
		{
			if (!IsActive)
				return;
			
			if (selectable?.TargetView == null
			    || !selectable.TargetView.TryGetComponent(out IRuntimeObjectView target)
			    || !IsAllowedTarget(target))
			{
				OnInterrupted();
				return;
			}

			Current.Assign(target);
			SelectTarget(true, target);
			
			if (Current.AdmissibleCount != Current.Targets.Count)
				return;

			selectionTask.TrySetResult(Current.Targets);
			CleanupSystem();
		}

		private void OnDeselected(ISelectable selectable)
		{
			OnInterrupted();
		}

		private void OnInterrupted()
		{
			if (!IsActive)
				return;

			Cancel();
		}

		private Vector3 GetDragPosition()
		{

			//if perspective camera do this =>
			var screenPos = pointer.ReadValue<Vector2>();

			// Use the source object's depth so the drag plane is consistent
			// with where the card lives in world space
			var fromWorldPos = Current?.From?.SelfContainer?.position ?? Vector3.zero;
			float depth = gameCamera.WorldToScreenPoint(fromWorldPos).z;

			return gameCamera.ScreenToWorldPoint(
				new Vector3(screenPos.x, screenPos.y, depth)
			);

			//if ortographic camera do this =>
			var position = pointer.ReadValue<Vector2>();
			return gameCamera.ScreenToWorldPoint(position);
		}
	}
}