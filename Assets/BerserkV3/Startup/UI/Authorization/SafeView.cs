using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RR.UI.FrameSystem;

namespace BerserkV3.Startup.UI
{
	[Obsolete("Instead of this use UISafeWindowBase")]
	public abstract class SafeView : BaseView
	{
		protected virtual int InteractableDelayMs => 500;
		private CancellationTokenSource blockInput;

		protected override void OnAwake()
		{
			base.OnAwake();
			Closed += OnClose;
			Shown += OnShow;
			Hidden += OnClose;
		}
		
		private void OnDestroy()
		{
			blockInput?.Cancel();
			blockInput?.Dispose();
			blockInput = null;
			Closed -= OnClose;
			Shown -= OnShow;
		}

		private void OnShow()
		{
			SetInteractableAsync(true).Forget();
		}

		private void OnClose()
		{
			SetInteractableAsync(false).Forget();
		}

		public async UniTask SetInteractableAsync(bool value)
		{
			try
			{
				blockInput?.Cancel();
				blockInput?.Dispose();
				blockInput = null;

				if (value)
				{
					blockInput = new CancellationTokenSource();
					await UniTask.Delay(InteractableDelayMs, cancellationToken: blockInput.Token);
				}

				if (CanvasGroup)
					CanvasGroup.interactable = value;
			}
			catch
			{
				// Nothing to do
			}
		}
	}
}