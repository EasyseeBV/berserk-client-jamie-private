using System.Threading;
using Cysharp.Threading.Tasks;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Common.UIService
{
	public abstract class UISafeWindowBase : UIWindowBase
	{
		protected virtual int InteractableDelayMs => 500;
		private CancellationTokenSource blockInput;
		protected CanvasGroup CanvasGroup;

		protected override void OnInit()
		{
			if(!TryGetComponent(out CanvasGroup))
				CanvasGroup = gameObject.AddComponent<CanvasGroup>();
			
			base.OnInit();
		}

		protected override void OnDisposed()
		{
			blockInput?.Cancel();
			blockInput?.Dispose();
			blockInput = null;
			base.OnDisposed();
		}

		public override void Show()
		{
			SetInteractableAsync(true).Forget();
			base.Showed();
		}

		public override void Hide()
		{
			SetInteractableAsync(false).Forget();
			base.Hide();
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