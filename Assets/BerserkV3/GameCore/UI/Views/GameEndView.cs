using System;
using System.Threading;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Startup;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface IGameEndView
	{
		event Action OnCommend;
		event Action OnPlayAgain;
		event Action OnMenu;
		void Show();
		void SetReason(string value);
		void SetAllowCommend(bool value);
		void SetAllowPlayAgain(bool value);
		void PlayCommend(bool victory, string text);
		UniTask SetPlayerDefeatAsync(
			string userName,
			string artUrl,
			string artMaskUrl,
			string frameArtUrl,
			string mmrText,
			CancellationToken token = default);

		UniTask SetPlayerVictoryAsync(
			string userName,
			string artUrl,
			string artMaskUrl,
			string frameArtUrl,
			string mmrText,
			CancellationToken token = default);
	}

	public partial class GameEndView : SafeView, IGameEndView
	{
		protected override int InteractableDelayMs => Mathf.RoundToInt((SCALE_DURATION + SCALE_DELAY) * 1000);
		private const float SCALE_DURATION = 1.8f;
		private const float FADE_DURATION = 0.3f;
		private const float EARLY_SCALE_DELAY = 0.2f;
		private const float SCALE_DELAY = 0.5f;

		public event Action OnCommend;
		public event Action OnPlayAgain;
		public event Action OnMenu;
		
		private IDisposable subscription;

		protected override void OnAwake()
		{
			base.OnAwake();
			
			subscription = MenuButton.SetHintTarget(TutorialTrigger.EndGame.ToString()).SetTransitionFactorSize().Init().Subscribe(MenuButton.Button);
			MenuButton.Subscribe(() => OnMenu?.Invoke());
			CommendButton.Subscribe(() => OnCommend?.Invoke());
			PlayAgainButton.Subscribe(() => OnPlayAgain?.Invoke());
			SetActive(DefeatCommendText, false);
			SetActive(VictoryCommendText, false);
			
			PlayerVictoryLayout.RectTransform.localScale = Vector3.zero;
			PlayerDefeatLayout.RectTransform.localScale = Vector3.zero;
			VersusLayout.RectTransform.localScale = Vector3.zero;
			PlayAgainButton.RectTransform.localScale = Vector3.zero;
			CommendButton.RectTransform.localScale = Vector3.zero;
			MenuButton.RectTransform.localScale = Vector3.zero;
		}

		protected override void OnShown()
		{
			base.OnShown();
			CanvasGroup.DOFade(1f, FADE_DURATION).From(0f);
			AnimateView(PlayerVictoryLayout, EARLY_SCALE_DELAY, Ease.OutElastic);
			AnimateView(PlayerDefeatLayout, EARLY_SCALE_DELAY * 2, Ease.OutElastic);
			AnimateView(VersusLayout, EARLY_SCALE_DELAY * 3, Ease.OutElastic);
			AnimateView(CommendButton, SCALE_DELAY, Ease.OutBounce);
			AnimateView(PlayAgainButton, SCALE_DELAY, Ease.OutBounce);
			AnimateView(MenuButton, SCALE_DELAY, Ease.OutBounce);
		}

		private void AnimateView(BaseView view, float delay, Ease ease)
		{
			if (!view || !view.gameObject.activeSelf)
				return;
			
			view.RectTransform.DOScale(Vector3.one, SCALE_DURATION)
				.From(Vector3.zero)
				.SetDelay(delay)
				.SetEase(ease);
		}

		public void SetReason(string value)
		{
			if (!EndGameReasonText)
				return;
			
			SetActive(EndGameReasonText, !string.IsNullOrWhiteSpace(value));
			EndGameReasonText.SetText(value);
		}

		public void SetAllowCommend(bool value)
		{
			if (CommendButton)
				SetActive(CommendButton, value);
		}

		public void SetAllowPlayAgain(bool value)
		{
			if (PlayAgainButton)
				SetActive(PlayAgainButton, value);
		}

		public async UniTask SetPlayerDefeatAsync(
			string userName, 
			string artUrl, 
			string artMaskUrl, 
			string frameArtUrl, 
			string mmrText,
			CancellationToken token = default)
		{
			await UniTask.WhenAll(
				PlayerDefeatLayout.SetArtMaskAsync(artMaskUrl, token),
				PlayerDefeatLayout.SetArtAsync(artUrl, token),
				PlayerDefeatLayout.SetFrameArtAsync(frameArtUrl, token))
				.AttachExternalCancellation(token)
				.SuppressCancellationThrow();
			
			if (token.IsCancellationRequested)
				return;
			PlayerDefeatLayout.SetStateText("Defeat");
			VersusLayout.SetPlayer1MmrText(mmrText);
			VersusLayout.SetPlayer1NameText(userName);
		}

		public async UniTask SetPlayerVictoryAsync(
			string userName, 
			string artUrl, 
			string artMaskUrl, 
			string frameArtUrl, 
			string mmrText,
			CancellationToken token = default)
		{
			await UniTask.WhenAll(
					PlayerVictoryLayout.SetArtMaskAsync(artMaskUrl, token),
					PlayerVictoryLayout.SetArtAsync(artUrl, token),
					PlayerVictoryLayout.SetFrameArtAsync(frameArtUrl, token))
				.AttachExternalCancellation(token)
				.SuppressCancellationThrow();
			
			if (token.IsCancellationRequested)
				return;
			PlayerVictoryLayout.SetStateText("Victory");
			VersusLayout.SetPlayer2MmrText(mmrText);
			VersusLayout.SetPlayer2NameText(userName);
		}
		
		public void PlayCommend(bool victory, string text)
		{
			var targetText = victory ? VictoryCommendText : DefeatCommendText;
			if (!targetText)
				return;
			
			targetText.SetText(text);
			var anchoredPosition = targetText.rectTransform.anchoredPosition;
			var target = new Vector2(anchoredPosition.x, 80);
			SetActive(targetText, true);
			DOTween.Sequence()
				.Append(targetText.rectTransform.DOAnchorPos(target, 1f))
				.Append(targetText.DOFade(0, 1f).From(1f))
				.OnKill(ResetText)
				.OnComplete(ResetText);
			
			return;
			void ResetText()
			{
				if (!targetText)
					return;
				
				targetText.rectTransform.anchoredPosition = anchoredPosition;
				SetActive(targetText, false);
			}
		}

		private void OnDestroy()
		{
			CommendButton.UnSubscribeAll();
			PlayAgainButton.UnSubscribeAll();
			MenuButton.UnSubscribeAll();
			OnCommend = null;
			OnPlayAgain = null;
			OnMenu = null;
			subscription?.Dispose();
			subscription = null;
		}
	}
}