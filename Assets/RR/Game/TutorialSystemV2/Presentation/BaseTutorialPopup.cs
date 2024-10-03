using System;
using System.Threading.Tasks;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Presentation
{
	public abstract class BaseTutorialPopup : BaseView, 
		ITutorialInvokeHandler, ITutorialCloseHandler, ITutorialRemoveHandler, ITutorialOrderable
	{
		public virtual int Order => 10;

		protected override void Start()
		{
			base.Start();
			TutorialAdapter.HandlersRepository.Register(this);
			Init();
		}

		public virtual async Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (hint?.Popup is not {Enabled: true})
				return;

			EnableBackHinder(hint.Popup.BackHinder);
			EnableBackButton(hint.Popup.BackButton);
			SetupText(TutorialAdapter.TextFormatter.Format(hint.Popup.Text));
			SetupTitle(TutorialAdapter.TextFormatter.Format(hint.Popup.Title));
			EnableArrow(hint.Popup.ArrowRequired);
			SetupPosition(hint.Popup.Position);
			SetupSize(hint.Popup.SizeDelta, hint.Popup.AutoSize);
			SetupContinueButton(hint.Popup.ContinueButton, 
				TutorialAdapter.TextFormatter.Format(hint.Popup.ContinueButtonText), HandleClick);
			
			if (hint.Popup.DelayBeforShow > 0)
				await Task.Delay(TimeSpan.FromSeconds(hint.Popup.DelayBeforShow));
			
			await ShowViewAsync();
		}

		public virtual async Task CloseAsync(ITutorialHintEntity hint)
		{
			if (hint?.Popup is not {Enabled: true})
				return;
			
			await CloseViewAsync();
			ResetToDefault();
		}

		public virtual async Task RemovedAsync()
		{
			await CloseViewAsync();
			ResetToDefault();
			UnInit();
		}

		protected virtual void HandleClick()
		{
			TutorialAdapter.Application.CloseAsync().Forget();
		}

		protected abstract void Init();

		protected abstract void UnInit();

		protected abstract void ResetToDefault();
		
		protected abstract void EnableArrow(bool value);
		
		protected abstract void EnableBackHinder(bool value);
		
		protected abstract void EnableBackButton(bool value);

		protected abstract void SetupContinueButton(bool enabled, string text, Action callBack);

		protected abstract void SetupPosition(Vector2 value);

		protected abstract void SetupSize(Vector2 value, bool autoSize);

		protected abstract void SetupTitle(string value);

		protected abstract void SetupText(string value);

		protected abstract Task ShowViewAsync();

		protected abstract Task CloseViewAsync();

		protected virtual void OnDestroy()
		{
			TutorialAdapter.HandlersRepository.UnRegister(this);
		}
	}
}