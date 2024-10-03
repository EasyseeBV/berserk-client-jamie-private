using System;
using System.Threading;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.TutorialSystem;
using Cysharp.Threading.Tasks;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface IMulliganView
	{
		event Action OnAccepted;
		Transform MulliganContainer { get; }
		Transform MulliganReplaceContainer { get; }
		
		IMulliganView SetConfirmButtonVisible(bool value);

		IMulliganView SetWaitingOpponentLabelVisible(bool value);

		IMulliganView SetCardsContainerInteractable(bool value);

		UniTask InitAndShowAsync(bool isFirstMover, Func<int> timer);

		UniTask CloseAsync();
	}
	
	public partial class MulliganView : BaseView, IMulliganView
	{
		[SerializeField] private CanvasGroup cardsGroup;
		private CancellationTokenSource timeProcess;
		private IDisposable acceptSubscription;
		
		public event Action OnAccepted;
		public Transform MulliganContainer => CardsContainer;
		public Transform MulliganReplaceContainer => ReplaceCardsContainer;
		protected override void OnAwake()
		{
			base.OnAwake();
			acceptSubscription = DoneBtn
				.SetHintTarget(TutorialTrigger.MulliganAccept.ToString())
				.SetTransitionFactorSize()
				.Init()
				.Subscribe(DoneBtn);

			CardsContainer.SetHintTarget(TutorialTrigger.MulliganRect.ToString()).SetTransitionFactorSize().Init();
		}

		private async void ProcessTime(Func<int> timer, CancellationToken token)
		{
			var tick = TimeSpan.FromSeconds(0.99f);
			while (Application.isPlaying && !token.IsCancellationRequested)
			{
				try
				{
					await UniTask.Delay(tick, cancellationToken: token);
					Set(TimerText, string.Format(GameDataBaseAdapter.Instance.GetLocalization("MulliganStartsInBig"), timer?.Invoke()));
				}
				catch (Exception)
				{
					return;
				}
			}
		}
		
		public async UniTask InitAndShowAsync(bool isFirstMover, Func<int> timer)
		{
			var tcs = new UniTaskCompletionSource();
			DoneBtn.onClick.RemoveAllListeners();
			DoneBtn.onClick.AddListener(() => OnAccepted?.Invoke());
			
			SetActive(SelfTurnText, isFirstMover);
			SetActive(OpponentTurnText, !isFirstMover);
			timeProcess?.Cancel();
			timeProcess?.Dispose();
			timeProcess = new CancellationTokenSource();
			ProcessTime(timer, timeProcess.Token);
			Show(onAnimationDone:() => tcs.TrySetResult());
			await tcs.Task;
		}

		public async UniTask CloseAsync()
		{
			var tcs = new UniTaskCompletionSource();
			Close(onAnimationDone:() => tcs.TrySetResult());
			await tcs.Task;
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			timeProcess?.Cancel();
			timeProcess?.Dispose();
			timeProcess = null;
			DoneBtn.onClick.RemoveAllListeners();
			OnAccepted = null;
		}

		public IMulliganView SetConfirmButtonVisible(bool value)
		{
			SetActive(DoneBtn, value);
			return this;
		}

		public IMulliganView SetWaitingOpponentLabelVisible(bool value)
		{
			SetActive(WaitingOpponentText, value);
			return this;
		}

		public IMulliganView SetCardsContainerInteractable(bool value)
		{
			cardsGroup.blocksRaycasts = value;
			return this;
		}

		private void OnDestroy()
		{
			acceptSubscription?.Dispose();
			timeProcess?.Cancel();
			timeProcess?.Dispose();
			acceptSubscription = null;
			timeProcess = null;
			DoneBtn.onClick.RemoveAllListeners();
			OnAccepted = null;
		}
	}
}