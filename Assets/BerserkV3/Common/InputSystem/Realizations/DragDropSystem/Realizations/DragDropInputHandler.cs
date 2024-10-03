using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public class DragDropInputHandler : IDragDropInputHandler
	{
		private readonly IInputController<DragSystemActions> inputController;
		private readonly IInputAction pointer;
		private readonly IInputAction initiation;
		private readonly IInputAction cancelation;
		private bool repeatInitiationAfterApplicationFocusChanged = true;

		public bool Enabled { get; private set; }
		public float BeginThreshold { get; set; } = 12f;
		public bool DragInitiated { get; private set; }
		public event Action OnStart;
		public event Action OnPreformed;
		public event Action OnEnded;
		public event Action OnCanceled;
		public Vector2? BeginPosition { get; private set; }
		public Vector2 Position { get; private set; }

		public DragDropInputHandler(IInputController<DragSystemActions> inputController)
		{
			this.inputController = inputController;
			pointer = inputController.Get(DragSystemActions.Pointer);
			initiation = inputController.Get(DragSystemActions.Initiation);
			cancelation = inputController.Get(DragSystemActions.Cancel);
			
			Application.focusChanged += OnApplicationFocusChanged;
			pointer.OnEnableChanged += OnInputActionEnableChanged;
			initiation.OnEnableChanged += OnInputActionEnableChanged;
			cancelation.OnEnableChanged += OnInputActionEnableChanged;
			OnInputActionEnableChanged();
		}

		public void Dispose()
		{
			Disable();
			pointer.OnEnableChanged -= Enable;
			initiation.OnEnableChanged -= Enable;
			cancelation.OnEnableChanged -= Enable;
			OnStart = null;
			OnPreformed = null;
			OnCanceled = null;
			OnEnded = null;
			Position = default;
		}

		public void Enable()
		{
			if (Enabled)
				return;
			
			if (pointer == null || initiation == null || cancelation == null)
				throw new NullReferenceException($"Cannot enable : input is missing!");
			
			Enabled = true;
			inputController.Enable();
			pointer.OnPerformed += OnDragMoved;
			initiation.OnPerformed += OnDragInitiation;
			initiation.OnCanceled += OnDragInitiation;
			cancelation.OnCanceled += OnDragCanceled;
			BeginPosition = null;
			DragInitiated = false;
		}

		public void Disable()
		{
			if (!Enabled)
				return;
			
			Enabled = false;
			if (pointer == null || initiation == null)
				throw new NullReferenceException($"Already disabled : input is missing!");
			
			Cancel();
			inputController.Disable();
			pointer.OnPerformed -= OnDragMoved;
			initiation.OnPerformed -= OnDragInitiation;
			initiation.OnCanceled -= OnDragInitiation;
			cancelation.OnCanceled -= OnDragCanceled;
		}

		public void Cancel()
		{
			BeginPosition = null;

			if (!DragInitiated) 
				return;
			
			DragInitiated = false;
			OnCanceled?.Invoke();
		}

		private void EndDrag()
		{
			BeginPosition = null;

			if (!DragInitiated) 
				return;
			
			DragInitiated = false;
			OnEnded?.Invoke();
		}
		
		private Vector2 GetPointerPosition()
		{
			return pointer.ReadValue<Vector2>();
		}
		
		private bool IsOutOfThreshold(Vector2 dragPosition)
		{
			return BeginPosition.HasValue && (BeginPosition.Value - dragPosition).magnitude >= BeginThreshold;
		}
		
		private async void OnDragInitiation(IInputContext inputContext)
		{
			if (inputContext.Performed && !BeginPosition.HasValue)
			{
				// https://issuetracker.unity3d.com/issues/input-system-the-first-touch-position-is-always-zero-when-using-mouse-emulation-for-touch-or-pen-1
				if (repeatInitiationAfterApplicationFocusChanged)
				{
					await Task.Yield();
					repeatInitiationAfterApplicationFocusChanged = false;
					OnDragInitiation(inputContext);
					return;
				}
				BeginPosition = GetPointerPosition();
				return;
			}

			if (inputContext.Canceled)
				EndDrag();
		}

		private void OnDragMoved(IInputContext inputContext)
		{
			var position = GetPointerPosition();

			if (!DragInitiated && IsOutOfThreshold(position))
			{
				DragInitiated = true;
				OnStart?.Invoke();
			}

			if (DragInitiated)
			{
				Position = position;
				OnPreformed?.Invoke();
			}
		}
		
		private void OnDragCanceled(IInputContext context)
		{
			if (DragInitiated)
				Cancel();
		}
		
		private void OnInputActionEnableChanged()
		{
			if (!pointer.Enabled || !initiation.Enabled || !cancelation.Enabled)
			{
				Disable();
				return;
			}
			
			Enable();
		}

		private void OnApplicationFocusChanged(bool focus)
		{
			repeatInitiationAfterApplicationFocusChanged = true;
		}
	}
}