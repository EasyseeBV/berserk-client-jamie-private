using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.LogicEvents.AutoBot;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using Cysharp.Threading.Tasks;
using Events;
using RR.Core.DebugSystem;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public class AutoPilotController : DisposableWithCts, IInitializable
	{
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameRepository gameRepository;
		private readonly IGameHub gameHub;
		private bool requestInFlight;
		private bool hubConnected;
		private DateTime lastRequestAt = DateTime.MinValue;

		public AutoPilotController(
			IGameLogicEventsSource gameLogicEventsSource,
			IGameRepository gameRepository,
			IGameHub gameHub)
		{
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameRepository = gameRepository;
			this.gameHub = gameHub;
		}

		public void Initialize()
		{
			LocalAutoPilot.ResetForSession();
			gameHub.OnConnectedSuccess += OnGameHubConnected;
			gameHub.OnConnectionClosed += OnGameHubDisconnected;
			gameHub.OnRestartRequired += OnGameHubRestartRequired;
			gameLogicEventsSource.Subscribe<MulliganStart>(_ => TryAutoAsync("mulligan").Forget(), Token);
			gameLogicEventsSource.Subscribe<TurnGame>(OnTurnGame, Token);
			gameLogicEventsSource.Subscribe<AutoBotPossibleMove>(OnAutoBotPossibleMove, Token);
			gameLogicEventsSource.Subscribe<EndGame>(_ =>
			{
				requestInFlight = false;
				hubConnected = false;
			}, Token);
			Token.Register(CleanupHubSubscriptions);
			RunAutoPilotLoopAsync().Forget();
		}

		private void OnGameHubConnected()
		{
			hubConnected = true;
		}

		private void OnGameHubDisconnected()
		{
			hubConnected = false;
			requestInFlight = false;
		}

		private void OnGameHubRestartRequired()
		{
			hubConnected = false;
			requestInFlight = false;
		}

		private void CleanupHubSubscriptions()
		{
			gameHub.OnConnectedSuccess -= OnGameHubConnected;
			gameHub.OnConnectionClosed -= OnGameHubDisconnected;
			gameHub.OnRestartRequired -= OnGameHubRestartRequired;
		}

		private void OnTurnGame(TurnGame turnGame)
		{
			if (turnGame.RuntimeData.OwnerId != gameRepository.SelfId)
				return;

			TryAutoAsync("turn").Forget();
		}

		private void OnAutoBotPossibleMove(AutoBotPossibleMove autoBotPossibleMove)
		{
			if (autoBotPossibleMove.TurnOwnerId != gameRepository.SelfId)
				return;

			TryAutoAsync("chain").Forget();
		}

		private async UniTaskVoid TryAutoAsync(string reason)
		{
			if (!LocalAutoPilot.Enabled || !LocalAutoPilot.IsAvailable)
				return;

			if (!hubConnected)
				return;

			if (requestInFlight)
				return;

			if ((DateTime.UtcNow - lastRequestAt).TotalMilliseconds < 250)
				return;

			requestInFlight = true;
			lastRequestAt = DateTime.UtcNow;

			try
			{
				await UniTask.Delay(TimeSpan.FromMilliseconds(350), cancellationToken: Token);
				await gameHub.AutoPerformCommandAsync();
				RRLogger.Log($"[AutoPilot] Requested auto move ({reason})");
			}
			catch (Exception exception)
			{
				RRLogger.Error($"[AutoPilot] Failed to request auto move ({reason}): {exception.Message}");
			}
			finally
			{
				requestInFlight = false;
			}
		}

		private async UniTaskVoid RunAutoPilotLoopAsync()
		{
			try
			{
				while (!Token.IsCancellationRequested)
				{
					await UniTask.Delay(TimeSpan.FromMilliseconds(900), cancellationToken: Token);

					if (!CanAutoPoll())
						continue;

					TryAutoAsync("poll").Forget();
				}
			}
			catch (OperationCanceledException)
			{
				// session disposed
			}
		}

		private bool CanAutoPoll()
		{
			if (!LocalAutoPilot.Enabled || !LocalAutoPilot.IsAvailable)
				return false;

			if (!hubConnected || requestInFlight)
				return false;

			if (GameBus.CurrentRound == null || GameBus.CurrentRound.Value == null)
				return false;

			if (GameBus.CurrentRound.Value.TurnOwner != Owner.Self)
				return false;

			return GameBus.RoundTimer == null || GameBus.RoundTimer.Value > 0;
		}
	}
}
