using System;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Repository;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{

	public class ConnectionHandler : IInitializable, IDisposable
	{
		private readonly ISharedTime sharedTime;
		private readonly IGameContext gameContext;
		private readonly ISharedConfig sharedConfig;
		private readonly IGameRepository gameRepository;
		private readonly IProgressDrawer progressDrawer;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		
		private CancellationTokenSource timerSource;
		private CancellationTokenSource connectionSource;
		private IProgress<float> opponentProgress;
		private IProgress<float> selfProgress;

		public event Action<string> OnFailedConnection; 

		public ConnectionHandler(
			ISharedTime sharedTime,
			IGameContext gameContext,
			ISharedConfig sharedConfig,
			IGameRepository gameRepository,
			IProgressDrawer progressDrawer,
			IGameLogicEventsSource gameLogicEventsSource)
		{
			this.sharedTime = sharedTime;
			this.gameContext = gameContext;
			this.sharedConfig = sharedConfig;
			this.gameRepository = gameRepository;
			this.progressDrawer = progressDrawer;
			this.gameLogicEventsSource = gameLogicEventsSource;
		}

		public void Initialize()
		{
			connectionSource = new CancellationTokenSource();
			selfProgress = progressDrawer.CreateProgress();
			opponentProgress = progressDrawer.CreateProgress();
			
			gameLogicEventsSource.Subscribe<ChangeConnectionStatus>(OnChangedConnectionStatus, connectionSource.Token);
			
			var connectionTimeout = sharedTime.Current + TimeSpan.FromSeconds(sharedConfig.GameConnectionTimeout);
			Processing(gameContext.GameDatabase.GetLocalization("GameStartConnection"), connectionTimeout, selfProgress, gameContext.GameDatabase.GetLocalization("GameStartNotConnected")).Forget();
		}

		public void Dispose()
		{
			if (connectionSource == null)
				return;
			
			timerSource?.Cancel();
			timerSource?.Dispose();
			connectionSource?.Cancel();
			connectionSource?.Dispose();
			selfProgress?.Report(1f);
			opponentProgress?.Report(1f);
			selfProgress = null;
			opponentProgress = null;
			connectionSource = null;
			OnFailedConnection = null;
			timerSource = null;
		}

		private void OnChangedConnectionStatus(ChangeConnectionStatus data)
		{
			if (data.UserId == User.Id || !gameRepository.Initialized)
				return;

			if (!data.IsConnected && (!gameContext.PlayerRepository.TryGet(data.UserId, out var runtimePlayer) || !runtimePlayer.RuntimeData.IsReady))
			{
				Processing(gameContext.GameDatabase.GetLocalization("GameStartOpponentConnection"), data.ConnectionTimout, opponentProgress, gameContext.GameDatabase.GetLocalization("GameStartOpponentNotConnected")).Forget();
				return;
			}

			Dispose();
		}

		private async UniTask Processing(string tooltip, DateTime endTime, IProgress<float> progress, string error)
		{
			timerSource?.Cancel();
			timerSource?.Dispose();
			timerSource = null;
			
			var from = (float)(endTime - sharedTime.Current).TotalSeconds;
			if (from <= 0)
			{
				OnFailedConnection?.Invoke(error);
				return;
			}

			timerSource = new CancellationTokenSource();
			var refreshTime = TimeSpan.FromSeconds(1f);
			var token = timerSource.Token;

			for (var timer = from; !token.IsCancellationRequested && timer > 0; timer--)
			{
				progressDrawer.SetTooltip(string.Format(tooltip, TimeSpan.FromSeconds(timer).ToString("mm':'ss").Replace(":",":".Size("125%"))));
				progress?.Report((from - timer) / from);
				await UniTask.Delay(refreshTime, cancellationToken: token).SuppressCancellationThrow();
			}
			
			if (token.IsCancellationRequested)
			{
				if (Application.isPlaying)
					progressDrawer.SetTooltip(null);
				
				return;
			}
			
			progress?.Report(1f);
			OnFailedConnection?.Invoke(error);
		}
	}

}