using System;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.AiBehaviour.Commands;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;

namespace Berserk.Shared.GameCore.AiBehaviour.Controllers
{
	public interface IGameBotSessionProcessor : IDisposable
	{
		ISessionProcessor SessionProcessor { get; }
		IAiBehaviourExecutor BehaviourExecutor { get; }
		PracticeMode PracticeMode { get; }
		string BotId { get; }

		IGameBotSessionProcessor Build();
	}

	public class GameBotSessionProcessor : IGameBotSessionProcessor
	{
		private CancellationTokenSource subscriptions;
		public ISessionProcessor SessionProcessor { get; }
		public IAiBehaviourExecutor BehaviourExecutor { get; }
		public PracticeMode PracticeMode { get; }
		public string BotId { get; }

		public GameBotSessionProcessor(
			ISessionProcessor sessionProcessor,
			IAiBehaviourExecutor behaviourExecutor,
			string botUserId,
			PracticeMode? practiceMode)
		{
			BotId = botUserId;
			PracticeMode = practiceMode ?? PracticeMode.Normal;
			SessionProcessor = sessionProcessor;
			BehaviourExecutor = behaviourExecutor;
			subscriptions = new CancellationTokenSource();
		}
		
		public IGameBotSessionProcessor Build()
		{
			SessionProcessor.Context.SharedEventsSource.Subscribe<NextTurnEvent>(OnExecute, subscriptions.Token, int.MaxValue);
			SessionProcessor.Context.SharedEventsSource.Subscribe<AiCommandExecuteEvent>(OnExecute, subscriptions.Token, int.MaxValue);
			OnExecute();
			return this;
		}
		
		private void OnExecute()
		{
			try
			{
				if (SessionProcessor.Context.Timer.RuntimeData == null
				    || SessionProcessor.Context.Timer.RuntimeData.OwnerId != BotId
				    || SessionProcessor.Context.Timer.RuntimeData.State != TimerState.Game)
					return;
				
				ExecuteTurnLogic();
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}

		private void ExecuteTurnLogic()
		{
			try
			{
				var aiBehavior = SessionProcessor.Context.GameDatabase.GetAiBehaviourNode(PracticeMode);
				BehaviourExecutor.ExecuteNode(aiBehavior);

				if (!SessionProcessor.Context.PlayerRepository.TryGet(BotId, out var player))
					return;
				
				SessionProcessor.LogicContext.LogicQueueController.Add(new ChangePlayerState(player.RuntimeData));
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}

		public void Dispose()
		{
			subscriptions?.Cancel();
			subscriptions?.Dispose();
			subscriptions = null;
		}
	}
}