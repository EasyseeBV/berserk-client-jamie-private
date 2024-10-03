using System;
using BerserkV3.Common.TutorialSystem;
using DG.Tweening;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BerserkV3.GameCore.UI
{
	public interface ITimerView
	{
		event Action OnPassingTurn;
		void ActiveWaitingState(bool value);
		void ActiveEnemyState(bool value);
		void SetInteractable(bool value);
		void HandleTenSecondsLeft();
		void SetTimerText(string value);
	}

	public partial class TimerView : BaseView, IInitializable, ITimerView
	{
		[SerializeField] private TextMeshProUGUI TimerTxt;
		[SerializeField] private Button EndTurnBtn;
		[SerializeField] private CanvasGroup EnemyTurnPanel;
		[SerializeField] private CanvasGroup WaitingPanel;
		[SerializeField] private Image FxTimeEndImage;

		private Tween changeTween;
		private IDisposable subscription;
		public event Action OnPassingTurn;

		protected override void OnAwake()
		{
			base.OnAwake();
			EndTurnBtn.onClick.RemoveAllListeners();
			EndTurnBtn.onClick.AddListener(() => OnPassingTurn?.Invoke());
			subscription = this.SetHintTarget($"{TutorialTrigger.GameTimer}").SetTransitionFactorSize().Init().Subscribe(EndTurnBtn);
		}

		public void ActiveWaitingState(bool value)
		{
			AnimateGroup(WaitingPanel, value);
			SetActive(TimerTxt, !value);
		}

		public void ActiveEnemyState(bool value)
		{
			AnimateGroup(EnemyTurnPanel, value);
			SetActive(TimerTxt, value);
		}

		public void SetInteractable(bool value)
		{
			EndTurnBtn.enabled = value;
		}

		public void HandleTenSecondsLeft()
		{
			changeTween?.Kill();
			changeTween = DOTween.Sequence()
				.Append(FxTimeEndImage.DOFade(1f, 0.1f).SetEase(Ease.InExpo))
				.Append(FxTimeEndImage.DOFade(0f, 0.5f).SetEase(Ease.InOutBack))
				.Play();
		}

		public void SetTimerText(string value)
		{
			if (TimerTxt)
				Set(TimerTxt, value);
		}

		private void AnimateGroup(CanvasGroup canvasGroup, bool show)
		{
			var to = show ? 1f : 0f;
			canvasGroup.DOKill();
			canvasGroup.DOFade(to, 0.35f)
				.SetEase(Ease.InOutQuad)
				.SetAutoKill(true);
		}

		private void OnDestroy()
		{
			if (EndTurnBtn.Value())
				EndTurnBtn.onClick.RemoveAllListeners();

			subscription?.Dispose();
			changeTween?.Kill();
			subscription = null;
			changeTween = null;
			OnPassingTurn = null;
		}
	}
}