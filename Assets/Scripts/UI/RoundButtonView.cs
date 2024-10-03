using Audio;
using Berserk.Shared.Data.Enums;
using DG.Tweening;
using Events;
using RR.Core.Extensions;
using RR.UI.DataBinding;
using RR.UI.FrameSystem;
using UnityEngine;
using Vulcan.Audio;

namespace UI
{
	public partial class RoundButtonView : BaseView
	{
		private const int DontTriggerEventsIfLessSeconds = 3;
		
		private Tween changeTween;

		protected override void Start()
		{
			base.Start();

			GameBus.OnTimerStateUpdated.Subscribe(this, () => SetButtonState(GameBus.CurrentRound.Value.TurnOwner));
			GameBus.RoundTimer.Subscribe(this, time => time == 10, HandleTenSecondsLeft);
			GameBus.RoundTimer.Subscribe(this, time => time == 0, () => SetButtonState(Berserk.Shared.Data.Enums.Owner.None));
			GameBus.RoundTimer.Bind(TimerTxt);
			
			EndTurnBtn.Subscribe(FinishRound);
			EndTurnBtn.enabled = false;
		}

		private void SetButtonState(Owner turnOwner)
		{
			if(EndTurnBtn)
				EndTurnBtn.enabled = turnOwner == Berserk.Shared.Data.Enums.Owner.Self && !GameBus.OnTimerPaused;
			
			ToggleCanvasGroup(EnemyTurnPanel, turnOwner == Berserk.Shared.Data.Enums.Owner.Opponent);
			ToggleCanvasGroup(ChangingPanel, GameBus.OpponentDisconnected || turnOwner == Berserk.Shared.Data.Enums.Owner.None);
			if(TimerTxt)
				SetActive(TimerTxt, turnOwner != Berserk.Shared.Data.Enums.Owner.None);

			changeTween?.Kill();
			if(ChangeImage)
				changeTween = DOTween.Sequence()
				.Append(ChangeImage.DOFade(1f, 0.1f).SetEase(Ease.InExpo))
				.Append(ChangeImage.DOFade(0f, 0.5f).SetEase(Ease.InOutBack))
				.Play();
		}

		private void ToggleCanvasGroup(CanvasGroup canvasGroup, bool show)
		{
			var to = show
				? 1f
				: 0f;

			if (!canvasGroup) 
				return;
			
			canvasGroup.DOKill();
			canvasGroup
				.DOFade(to, 0.35f)
				.SetEase(Ease.InOutQuad);
		}

		private void FinishRound()
		{
			if (GameBus.OpponentDisconnected)
				return;
			
			SetButtonState(Berserk.Shared.Data.Enums.Owner.None);
			
			// it's a workaround to prevent exceptions when server switched round and then client request round switch
			// on server side round and round owner changed, but client don't know about it yet
			if (GameBus.RoundTimer.Value < DontTriggerEventsIfLessSeconds)
				return;
			
			GameBus.OnPassingTurned += true;
			GameBus.OnRoundEnd += true;
		}

		private void HandleTenSecondsLeft()
		{
			AudioController.Play(Clip.Timer_RunningOut);
		}
	}
}