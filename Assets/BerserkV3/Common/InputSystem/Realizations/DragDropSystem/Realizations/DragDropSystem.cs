using System;
using System.Collections.Generic;
using BerserkV3.Common.Utils;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem.Utils;
using UnityEngine;

namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public class DragDropSystem : DisposableWithCts, IDragDropSystem
	{
		private struct DraggableInfo
		{
			public IDraggable Draggable;
			public CanvasGroup DraggableGroup;
			public RectTransform DraggableRect;
			public Transform SourceParent;
			public Canvas ParentCanvas;

			public float SourceAlpha;
			public int SourceSiblingIndex;
			public bool SourceBlocksRaycasts;
			public Vector2 AnchoredPosition;
			public Vector2 SourceSizeDelta;
			public Vector2 SourceAnchorsMax;
			public Vector2 SourceAnchorsMin;
			public Vector3 SourceScale;
		}
		
		private readonly List<IDropHandler> dropHandlers;
		private readonly List<IDraggable> draggables;
		private DraggableInfo draggableInfo;

		public event Action<IDraggable> OnDragStart;
		public event Action<IDraggable> OnDrag;
		public event Action<IDraggable, IDropHandler> OnDragCanceled;
		public event Action<IDraggable, IDropHandler> OnDragIn;
		public event Action<IDraggable, IDropHandler> OnDragOut;
		public event Action<IDraggable, IDropHandler> OnDroppedIn;
		
		public IDragDropInputHandler InputHandler { get; }
		public IDraggable Current => draggableInfo.Draggable;

		public IDropHandler CurrentDropHandler { get; private set; }

		public DragDropSystem(IDragDropInputHandler inputHandler)
		{
			draggables = new List<IDraggable>();
			dropHandlers = new List<IDropHandler>();
			
			InputHandler = inputHandler;
			InputHandler.OnStart += OnInputDragInitiated;
			InputHandler.OnPreformed += OnInputDragUpdated;
			InputHandler.OnEnded += OnInputDragEnded;
			InputHandler.OnCanceled += Cancel;
		}

		public override void Dispose()
		{
			base.Dispose();
			Cancel();
			InputHandler.OnStart -= OnInputDragInitiated;
			InputHandler.OnPreformed -= OnInputDragUpdated;
			InputHandler.OnEnded -= OnInputDragEnded;
			InputHandler.OnCanceled -= Cancel;
			OnDragStart = null;
			OnDrag = null;
			OnDragCanceled = null;
			OnDragIn = null;
			OnDragOut = null;
			OnDroppedIn = null;
		}

		public void Registration(IDraggable value)
		{
			try
			{
				if (value == null)
					throw new NullReferenceException(
						$"{nameof(IDraggable)} - instance missing, could not be registered");

				if (draggables.Contains(value))
					throw new ArgumentException(
						$"{value.GetType().Name} : {nameof(IDraggable)} - instance is already registered");

				draggables.Add(value);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Registration(IDropHandler value)
		{
			try
			{
				if (value == null)
					throw new NullReferenceException(
						$"{nameof(IDropHandler)} - instance missing, could not be registered");

				if (dropHandlers.Contains(value))
					throw new ArgumentException(
						$"{value.GetType().Name} : {nameof(IDropHandler)} - instance is already registered");

				dropHandlers.Add(value);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void UnRegistration(IDraggable value)
		{
			try
			{
				if (Current == value)
					Cancel();

				draggables.Remove(value);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void UnRegistration(IDropHandler value)
		{
			try
			{
				if (CurrentDropHandler == value)
					OnDraggableOutDropHandler();

				dropHandlers.Remove(value);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Cancel()
		{
			try
			{
				if (Current == null)
					return;

				OnDraggableResetWhenCanceled();
				var draggable = Current;
				var dropHandler = CurrentDropHandler;
				draggableInfo = default;
				CurrentDropHandler = default;
				OnDragCanceled?.Invoke(draggable, dropHandler);
				InputHandler.Cancel();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Clear()
		{
			Cancel();
			dropHandlers.Clear();
			draggables.Clear();
			draggableInfo = default;

			OnDragStart = null;
			OnDrag = null;
			OnDragCanceled = null;
			OnDragIn = null;
			OnDragOut = null;
			OnDroppedIn = null;
			CurrentDropHandler  = null;
		}

		private void OnDraggableReset()
		{
			try
			{
				if (Current == null)
					return;

				draggableInfo.DraggableGroup.blocksRaycasts = draggableInfo.SourceBlocksRaycasts;
				UpdateDraggableAlpha(draggableInfo.SourceAlpha);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnDraggableResetWhenCanceled()
		{
			try
			{
				if (Current == null)
					return;

				OnDraggableReset();
				if (!Current.DragSettings.ResetOnCanceled)
					return;

				draggableInfo.DraggableRect.SetParent(draggableInfo.SourceParent,
					Current.DragSettings.WorldPositionStaysWhenDraggingCanceled);
				UpdateDraggablePosition(draggableInfo.AnchoredPosition);
				var draggableRect = draggableInfo.DraggableRect;

				draggableRect.SetSiblingIndex(draggableInfo.SourceSiblingIndex);

				if (!Current.DragSettings.WorldPositionStaysWhenDraggingCanceled)
				{
					draggableRect.sizeDelta = draggableInfo.SourceSizeDelta;
					draggableRect.anchorMax = draggableInfo.SourceAnchorsMax;
					draggableRect.anchorMin = draggableInfo.SourceAnchorsMin;
					draggableRect.localScale = draggableInfo.SourceScale;
				}
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnUpdateDragInOut()
		{
			try
			{
				if (Current == null)
					return;

				var unavailableAlpha = draggableInfo.SourceAlpha / Current.DragSettings.TransparencyAmount;
				if (CurrentDropHandler == null)
				{
					UpdateDraggableAlpha(unavailableAlpha);
					for (var i = 0; i < dropHandlers.Count; i++)
					{
						var dropHandler = dropHandlers[i];
						if (!InputHelper.IsPointerOver(dropHandler.DropHandlerView))
							continue;

						CurrentDropHandler = dropHandler;
						UpdateDraggableAlpha(CurrentDropHandler.CanDrop(Current)
							? draggableInfo.SourceAlpha
							: unavailableAlpha);
						OnDragIn?.Invoke(Current, CurrentDropHandler);
						break;
					}

					return;
				}

				if (InputHelper.IsPointerOver(CurrentDropHandler.DropHandlerView))
					return;

				OnDraggableOutDropHandler();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnDraggableOutDropHandler()
		{
			try
			{
				if (Current == null)
					return;

				var unavailableAlpha = draggableInfo.SourceAlpha / Current.DragSettings.TransparencyAmount;
				var dropHandlerResult = CurrentDropHandler;
				UpdateDraggableAlpha(unavailableAlpha);
				CurrentDropHandler = null;
				OnDragOut?.Invoke(Current, dropHandlerResult);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void UpdateDraggableAlpha(float value)
		{
			try
			{
				if (Current == null || !Current.DragSettings.InOutTransparency)
					return;

				draggableInfo.DraggableGroup.alpha = value;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void UpdateDraggablePosition(Vector3? position = null)
		{
			try
			{
				if (Current == null)
					return;

				draggableInfo.DraggableRect.anchoredPosition =
					position ?? draggableInfo.ParentCanvas.ScreenToCanvasPosition(InputHandler.Position);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnInputDragInitiated()
		{
			// do not to use foreach-loop 
			foreach (var draggable in draggables)
			{
				try
				{
					if (!draggable.DragSettings.Enable
					    || !InputHelper.IsPointerOver(draggable.TargetView)
					    || !draggable.CanDrag()) 
						continue;
					
					var canvasGroup = draggable.TargetView.GetOrAddComponent<CanvasGroup>();
					var draggableTransform = draggable.TargetView.GetComponent<RectTransform>();
					var parentCanvas = draggable.TargetView.GetComponentInParent<Canvas>();

					draggableInfo = new DraggableInfo
					{
						Draggable = draggable,
						DraggableGroup = canvasGroup,
						DraggableRect = draggableTransform,
						SourceParent = draggableTransform.parent,
						ParentCanvas = parentCanvas,
						SourceAlpha = canvasGroup.alpha,
						SourceSiblingIndex = draggableTransform.GetSiblingIndex(),
						AnchoredPosition = draggableTransform.anchoredPosition,
						SourceSizeDelta = draggableTransform.sizeDelta,
						SourceAnchorsMax = draggableTransform.anchorMax,
						SourceAnchorsMin = draggableTransform.anchorMin,
						SourceScale = draggableTransform.localScale,
						SourceBlocksRaycasts = canvasGroup.blocksRaycasts
					};

					canvasGroup.blocksRaycasts = false;
					draggableTransform.SetParent(parentCanvas.transform, draggable.DragSettings.WorldPositionStaysWhenDraggingStart);
					OnDragStart?.Invoke(draggable);
					return;
				}
				catch (Exception e)
				{
					RRLogger.Error(e);
				}
			}
		}

		private void OnInputDragUpdated()
		{
			try
			{
				if (Current == null)
					return;

				OnUpdateDragInOut();
				UpdateDraggablePosition();
				OnDrag?.Invoke(Current);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnInputDragEnded()
		{
			try
			{
				if (Current == null)
					return;

				if (CurrentDropHandler != null && CurrentDropHandler.CanDrop(Current))
				{
					var resultDraggable = Current;
					var resultDrophandler = CurrentDropHandler;
					OnDraggableReset();
					draggableInfo = default;
					CurrentDropHandler = default;
					OnDroppedIn?.Invoke(resultDraggable, resultDrophandler);
					return;
				}

				Cancel();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}
	}
}