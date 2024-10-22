using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.AudioSystem;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using GameCore;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public class TimerController : DisposableWithCts, IInitializable, ITickable
	{
		private readonly ITimerView timerView;
		private readonly IAudioApplication audioApplication;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameRepository gameRepository;
		private readonly IGameContext gameContext;
		private readonly IGameHub gameHub;
		private bool waitingForUpdate;
		private bool canHandleThenSeconds;

		public TimerController(
			ITimerView timerView,
			IAudioApplication audioApplication,
			IGameLogicEventsSource gameLogicEventsSource,
			IGameRepository gameRepository,
			IGameContext gameContext,
			IGameHub gameHub)
		{
			this.timerView = timerView;
			this.audioApplication = audioApplication;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameRepository = gameRepository;
			this.gameContext = gameContext;
			this.gameHub = gameHub;
		}

		public void Initialize()
		{
			UpdateState(true);
			gameLogicEventsSource.Subscribe<EndGame>(_ => UpdateState(true), Token);
			gameLogicEventsSource.Subscribe<ChangedTimer>(_ => UpdateState(), Token);
			gameLogicEventsSource.Subscribe<TurnGame>(OnTurnChanged, Token);
			timerView.OnPassingTurn += FinishRound;
		}

		public void Tick()
		{
			if (gameContext.Timer.RuntimeData == null || timerView == null || waitingForUpdate)
				return;

			var timeLeft = gameContext.Timer.GetTimeLeft();
			timerView.SetTimerText(timeLeft.ToString());

			if (canHandleThenSeconds && timeLeft == 10)
			{
				canHandleThenSeconds = false;
				audioApplication.PlaySound(Clip.Timer_RunningOut);
				timerView.HandleTenSecondsLeft();
				return;
			}

			if (timeLeft <= 0)
			{
				gameContext.Timer.SetOwner("");
				UpdateState(true);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			if(timerView != null)
				timerView.OnPassingTurn -= FinishRound;
		}
		
		private void UpdateState(bool forceWaiting = false)
		{
			if (timerView == null || gameContext.Timer.RuntimeData == null || gameContext.RuntimeData == null)
				return;
			
			waitingForUpdate = forceWaiting || gameContext.RuntimeData.IsEnded;
			var waiting = gameContext.Timer.RuntimeData.State is TimerState.Ended or TimerState.Mulligan || waitingForUpdate;
			var isSelf = gameContext.Timer.RuntimeData.OwnerId == gameRepository.SelfId;
			canHandleThenSeconds = isSelf;
			
			if (waiting)
				GameCoreBus.OnLocalTurnPassed += Owner.None;
			
			timerView.SetInteractable(!waiting && isSelf);
			timerView.ActiveEnemyState(!isSelf);
			timerView.ActiveWaitingState(waiting);
		}

		private void FinishRound()
		{
			if (gameContext.Timer.RuntimeData == null)
				return;
			
			if (gameContext.Timer.RuntimeData.OwnerId != gameRepository.SelfId)
				return;
			
			if (gameContext.Timer.GetTimeLeft() <= 0)
				return;
			
			UpdateState(true);
			gameHub.PerformCommandAsync<PassTurnCmd>().Forget(DefaultSharedLogger.Error);
		}
		
		private void OnTurnChanged(TurnGame data)
		{
			var localTimerOwner = gameRepository.GetOwnerByUserId(gameContext.Timer.RuntimeData.OwnerId);
			if (data.RuntimeData.OwnerId != gameContext.Timer.RuntimeData.OwnerId || GameCoreBus.OnLocalTurnPassed != localTimerOwner)
				GameCoreBus.OnLocalTurnPassed += localTimerOwner;
			
			UpdateState();
		}
	}
}