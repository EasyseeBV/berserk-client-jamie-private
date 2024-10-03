using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Prediction.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.Generic.UndoSystem;

namespace BerserkV3.GameCore.Prediction.Realizations
{
	public class LogicEventMeta
	{
		public Type TargetEvent { get; }
		public string[] Args { get; }

		public LogicEventMeta(Type targetEvent, params string[] args)
		{
			TargetEvent = targetEvent;
			Args = args;
		}
	}

	public class PredictProcessor : IPredictProcessor
	{
		private readonly IUndoSystem undoSystem;
		private readonly ISessionProcessor sessionProcessor;
		private readonly IGameLogicEventsProcessor gameLogicEventsProcessor;
		private readonly IGameRepository gameRepository;
		private readonly List<LogicEventMeta> predictedEvents;
		private int localRuntimeGenerator = -1;

		public PredictProcessor(
			IUndoSystem undoSystem,
			ISessionProcessor sessionProcessor,
			IGameLogicEventsProcessor gameLogicEventsProcessor,
			IGameRepository gameRepository)
		{
			this.undoSystem = undoSystem;
			this.sessionProcessor = sessionProcessor;
			this.gameLogicEventsProcessor = gameLogicEventsProcessor;
			this.gameRepository = gameRepository;
			predictedEvents = new List<LogicEventMeta>();
		}

		public void PerformCmd<T>(CmdParamsModel model) where T : Command
		{
			try
			{
				var cmdType = typeof(T);
				if (cmdType == typeof(PerformAttackCmd))
				{
					PerformAttackPredict(model);
					return;
				}

				var cmdName = cmdType.Name;
				sessionProcessor
					.LogicContext
					.CommandController
					.Execute(gameRepository.SelfId, cmdName, model);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				undoSystem.Undo(model.CommandId);
			}
		}

		public void ProcessPredictedQueue(IList<ILogicEvent> events)
		{
			foreach (var logicEvent in events.ToArray())
			{
				var predicted =
					predictedEvents.FirstOrDefault(meta => MatchLogicEventToPredictedMeta(logicEvent, meta));
				if (predicted == null)
					continue;

				events.Remove(logicEvent);
				predictedEvents.Remove(predicted);
			}
		}

		private bool MatchLogicEventToPredictedMeta(ILogicEvent logicEvent, LogicEventMeta predicted)
		{
			if (predicted.TargetEvent != logicEvent.GetType())
				return false;

			return logicEvent switch
			{
				StartEffect startEffect => predicted.Args.All(arg => startEffect.RuntimeData.ConfigId == arg 
				                                                     || startEffect.RuntimeData.ExecutorId.ToString() == arg),
				
				EndEffect endEffect => predicted.Args.All(arg => endEffect.RuntimeData.Id.ToString() == arg 
				                                                 || endEffect.RuntimeData.ExecutorId.ToString() == arg),
				_ => false
			};
		}

		private void PerformAttackPredict(CmdParamsModel model)
		{
			var configId = EffectKeyword.GenericAttack.AsSystemEffectId();
			var effectData = sessionProcessor.Context.GameDatabase.GetEffectConfig(configId);
			var runtimeData = new RuntimeEffectData
			{
				Id = localRuntimeGenerator--, // just fake uniq id
				ConfigId = configId,
				TargetIds = model.TargetObjectsIds,
				ExecutorId = model.ExecutorObjectId,
				CurrentValue = effectData.Value,
				CurrentLength = effectData.Length
			};
			var predictLogicEvents = new ILogicEvent[]
			{
				new StartEffect(runtimeData),
				new EndEffect(runtimeData),
			};

			predictedEvents.AddRange(predictLogicEvents.Select(logic => new LogicEventMeta(logic.GetType(), configId, model.ExecutorObjectId.ToString())));
			gameLogicEventsProcessor.Process(predictLogicEvents);
		}
	}
}