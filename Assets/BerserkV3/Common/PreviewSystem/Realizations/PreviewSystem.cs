using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Zenject;

namespace BerserkV3.Common.PreviewSystem
{

	public class PreviewSystem : IPreviewSystem, IInitializable, IDisposable
	{
		private readonly IPreviewView previewView;
		private readonly IInputController<PreviewType> inputController;
		private readonly List<IPreviewable> registered;
		private bool isLocked;
		public IPreviewable Current { get; private set; }
		public event Action OnPreview;
		public event Action OnClose;

		public PreviewSystem(
			IInputController<PreviewType> inputController,
			IPreviewView previewView)
		{
			this.inputController = inputController;
			this.previewView = previewView;
			registered = new List<IPreviewable>();
		}
		
		public void Initialize()
		{
			Application.focusChanged += OnApplicationFocusChanged;
			inputController.GetAll().ForEach(input =>
			{
				input.OnStarted += OnPreviewInput;
				input.OnPerformed += OnPreviewInput;
				input.OnCanceled += OnPreviewInput;
			});
		}
		
		public void Dispose()
		{
			Current = null;
			OnPreview = null;
			OnClose = null;
			Application.focusChanged -= OnApplicationFocusChanged;
			inputController.GetAll().ForEach(input =>
			{
				input.OnStarted -= OnPreviewInput;
				input.OnPerformed -= OnPreviewInput;
				input.OnCanceled -= OnPreviewInput;
			});
		}

		public void Registration(IPreviewable previewable)
		{
			if (previewable == null)
				throw new NullReferenceException($"Target {nameof(IPreviewable)} is missing");

			if (registered.Contains(previewable))
				throw new InvalidOperationException($"{previewable} already registered");
			
			registered.Add(previewable);
		}

		public void UnRegistration(IPreviewable previewable)
		{
			if (previewable == null)
				throw new NullReferenceException($"Target {nameof(IPreviewable)} is missing");

			if (!registered.Contains(previewable))
				return;
			
			if (Current == previewable)
				Close();
			
			registered.Remove(previewable);
		}
		
		public void Preview(IPreviewable previewable)
		{
			if (Current != null)
				Close();

			if (isLocked || previewable == null || !previewable.PreviewSettings.Enabled)
				return;

			if (!registered.Contains(previewable))
				throw new InvalidOperationException($"{previewable} does not registered");

			Current = previewable;
			switch (Current.PreviewSettings.PreviewType)
			{
				case PreviewType.Pile:
				case PreviewType.Hand:
				case PreviewType.Graveyard:
					previewView.InitAndShowCenter(Current.PreviewData, Current.PreviewSettings.PreviewTime);
					break;
				
				case PreviewType.Creature:
					previewView.InitAndShowSide(Current.PreviewData, Current.PreviewSettings.PreviewTime);
					break; 
				
				case PreviewType.Fit:
					previewView.ShowAndFitAroundTarget(Current.TargetView, Current.PreviewData, Current.PreviewSettings.PreviewTime);
					break; 
				case PreviewType.DeckValueInfo:
					previewView.ShowDeckValueInfo(Current.TargetView, Current.PreviewData, Current.PreviewSettings.PreviewTime);
					break;
				
				default: previewView.InitAndShowCorner(Current.PreviewData, Current.PreviewSettings.PreviewTime);
					break;
			}
			OnPreview?.Invoke();
		}

		public void Lock(bool value)
		{
			isLocked = value;
		}

		public void Close()
		{
			if (Current == null) 
				return;

			Current = null;
			previewView.Close();
			OnClose?.Invoke();
		}

		public void Clear()
		{
			Close();
			registered.Clear();
			Current = null;
			OnPreview = null;
			OnClose = null;
		}

		private bool GetPreviewable(PreviewType type, out IPreviewable previewable)
		{
			return registered.ToArray().TryGet(x => x.PreviewSettings.PreviewType == type
													&& x.PreviewSettings.Enabled 
													&& x.CanPreview()
													&& InputHelper.IsPointerOver(x.TargetView), out previewable);
		}
		private int _hoverToken = 0;
		private IPreviewable _currentHover = null;

		private async void OnPreviewInput(IInputContext context)
		{
			if (!Enum.TryParse(context.Id, out PreviewType type))
			{
				RRLogger.Error($"[{GetType().Name}] Unknown input : {context.Id}");
				return;
			}

			// If preview is already open
			if (Current != null)
			{
				if (Current.PreviewSettings.PreviewType != type)
					return;

				if (context.Canceled || !InputHelper.IsPointerOver(Current.TargetView))
				{
					_hoverToken++; // invalidate any pending previews
					_currentHover = null;
					Close();
				}

				return;
			}

			// Hover started
			if ((context.Started || context.Performed) && GetPreviewable(type, out var previewable))
			{
				_currentHover = previewable;

				int token = ++_hoverToken; // create a new hover session

				await Task.Delay(500);

				// Only proceed if this hover session is still valid
				if (token == _hoverToken && _currentHover == previewable)
				{
					Preview(previewable);
				}
			}

			// Hover canceled explicitly
			if (context.Canceled)
			{
				_hoverToken++; // cancel pending delay
				_currentHover = null;
			}
		}

		private void OnApplicationFocusChanged(bool focus)
		{
			if (!focus)
				Close();
		}
	}
}