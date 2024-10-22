using System;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.Controllers
{
	public class TurnController : ITurnController
	{
		private readonly IGameContext gameContext;
		private readonly IGameLogicContext gameLogicContext;
		private CancellationTokenSource subscriptions;

		public TurnController(IGameContext gameContext, IGameLogicContext gameLogicContext)
		{
			this.gameContext = gameContext;
			this.gameLogicContext = gameLogicContext;
			subscriptions = new CancellationTokenSource();
		}

		public void Init(bool subscribe = true)
		{
			if (!subscribe)
				return;
			
			gameContext.SharedEventsSource.Subscribe<NextTurnEvent>(HandleNextTurn, subscriptions.Token);
			gameContext.SharedEventsSource.Subscribe<UserAfkEvent>(EndGameWhenUserAfk, subscriptions.Token);
			gameContext.SharedEventsSource.Subscribe<TimerChangedEvent>(OnTimerStateChanged, subscriptions.Token);
		}
		
		public void Dispose()
		{
			subscriptions?.Cancel();
			subscriptions?.Dispose();
			subscriptions = null;
		}

		private void OnTimerStateChanged()
		{
			gameLogicContext.LogicQueueController.Add(new ChangedTimer(gameContext.Timer.RuntimeData));
		}

		private void HandleNextTurn()
		{
			try
			{
				var timerData = gameContext.Timer.RuntimeData;
				var ownerId = timerData.OwnerId;
				var prevOwnerId = gameContext.PlayerRepository.GetOpposite(ownerId).UserId;

				SetNullMovesOnTable(prevOwnerId);
				
				if (timerData.Turn > 1)
					ExecuteAfterTurnEndedPhase(prevOwnerId, ownerId);
				
				ReplenishMana(ownerId, timerData.Turn, timerData.Round);

				if (timerData.Round == 1)
					ExecuteFirstRoundPhase(ownerId);

				// don't switch the order pls
				// it will cause expire problem because
				// ExecuteBeforeEachTurnStartedPhase only executes effects
				// but ExecuteBeforeTurnStartedPhase executes effects and does expire logic
				ExecuteBeforeEachTurnStartedPhase(ownerId, prevOwnerId);
				ExecuteBeforeTurnStartedPhase(ownerId, prevOwnerId);
				HandleNewTurnWithoutCards(ownerId);

				//The change of turn for the player must occur after the automatic phases of processing a new move.
				gameLogicContext.LogicQueueController.Add(new TurnGame(timerData));
				ExecuteGiveCards(ownerId, timerData.Turn);
				ExecuteRoundStartedPhase(ownerId);
				RestoreMovesOnTable(ownerId);
			}
			catch (Exception exception)
			{
				DefaultSharedLogger.Error(exception);
			}
		}

		private void HandleNewTurnWithoutCards(string ownerId)
		{
			if (!gameLogicContext.GiveCardsService.IsDeckEmpty(ownerId))
				return;

			var runtimePlayer = gameContext.PlayerRepository.Get(ownerId);
			runtimePlayer.AddTurnWitoutCards(1); 
		}

		private void ExecuteGiveCards(string ownerId, int turnNumber)
		{
			var cardsToDraw = turnNumber <= 13 ? 1 : 2;
			gameLogicContext.GiveCardsService.GiveCards(ownerId, cardsToDraw);
		}

		private void EndGameWhenUserAfk(UserAfkEvent data)
		{
			var param = new GameEndParams { Reason = GameEndReason.Afk };
			var model = new CmdParamsModel(data.RuntimeData.TimeHash, param);
			gameLogicContext.CommandController.Execute<GameEndCmd>(data.RuntimeData.OwnerId, model, true);
		}
		
		private void ExecuteBeforeEachTurnStartedPhase(string turnOwner, string prevOwner)
		{
			ExecutePhase(turnOwner, EffectPhase.BeforeEachRound);
			ExecutePhase(prevOwner, EffectPhase.BeforeEachRound);
		}

		private void ExecuteBeforeTurnStartedPhase(string turnOwner,string prevOwner)
		{
			ExecuteExpirePhase(turnOwner, ExpirePhase.BeforeRoundStarted);
			ExecuteExpirePhase(prevOwner, ExpirePhase.BeforeEachRound);
			ExecuteExpirePhase(turnOwner, ExpirePhase.BeforeEachRound);
			ExecutePhase(turnOwner, EffectPhase.BeforeNextRound);
		}

		private void ExecuteAfterTurnEndedPhase(string turnOwner, string prevOwner)
		{
			ExecuteExpirePhase(turnOwner, ExpirePhase.AfterRoundEnded);
			ExecuteExpirePhase(prevOwner, ExpirePhase.AfterEachRound);
			ExecuteExpirePhase(turnOwner, ExpirePhase.AfterEachRound);
			ExecutePhase(turnOwner, EffectPhase.BeforeEndRound);
		}

		private void ExecuteRoundStartedPhase(string turnOwner)
		{
			ExecutePhase(turnOwner, EffectPhase.RoundStarted);
		}

		private void ExecuteFirstRoundPhase(string turnOwner)
		{
			ExecutePhase(turnOwner, EffectPhase.FirstTurn);
		}

		private void ExecutePhase(string userId, EffectPhase phase)
		{
			if (string.IsNullOrEmpty(userId))
			{
				DefaultSharedLogger.Error($"[{GetType().Name}.{nameof(ExecutePhase)}] " +
				                          $"Can't execute phase : {phase}, because userId is missing.");
				return;
			}

			gameContext.GameRuntimePool.GetHeroByUserId(userId).ChangeEffectPhase(phase);
		}

		private void ExecuteExpirePhase(string userId, ExpirePhase expirePhase)
		{
			if (string.IsNullOrEmpty(userId))
			{
				DefaultSharedLogger.Error($"[{GetType().Name}.{nameof(ExpirePhase)}] " +
				                          $"Can't expire phase : {expirePhase}, because userId is missing.");
				return;
			}

			foreach (var gameObject in gameContext.GameRuntimePool.GetAllTableObjects(userId))
				gameLogicContext.EffectExecutor.ExpireAppliedEffects(gameObject, expirePhase);
		}

		private void RestoreMovesOnTable(string userId)
		{
			foreach (var card in gameContext.GameRuntimePool.GetAllTableObjects(userId))
				card.RuntimeData.MoveCount.Add(card.RuntimeData.MoveCount.BaseStat);
		}

		private void SetNullMovesOnTable(string userId)
		{
			foreach (var card in gameContext.GameRuntimePool.GetAllTableObjects(userId))
				card.RuntimeData.MoveCount.Set(0);
		}

		private void ReplenishMana(string userId, int turn, int round)
		{
			var lavaRound = gameContext.SharedConfig.PlayerManaRoundIncrease * round;
			// The first player always has less lava than the second player
			var currentLava = turn % 2 != 0 ? lavaRound : lavaRound + 1;

			var maxPlayerMana = gameContext.SharedConfig.PlayerMaxMana;
			var playerState = gameContext.PlayerRepository.Get(userId);
			var manaValue = Math.Min(currentLava, maxPlayerMana);
			playerState.RuntimeData.Mana.SetMax(manaValue).ResetToMax();
		}
	}
}