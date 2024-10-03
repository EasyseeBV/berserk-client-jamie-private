using System;
using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public interface IGameLocks
	{
		bool ByGlobal { get; }
		bool ByQueueProcessed { get; }
		event Action OnByQueueChanged;
	}
	
	public class GameLocks : DisposableWithCts, IInitializable, IGameLocks
	{
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameContext gameContext;
		private readonly List<string> queueStarted;

		public bool ByGlobal => gameContext.RuntimeData.IsEnded;
		public bool ByQueueProcessed => queueStarted.Count > 0 || ByGlobal;
		public event Action OnByQueueChanged;

		public GameLocks(IGameLogicEventsSource gameLogicEventsSource, IGameContext gameContext)
		{
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameContext = gameContext;
			queueStarted = new List<string>();
		}

		public void Initialize()
		{
			gameLogicEventsSource.Subscribe<CommandWait>(data => QueueStart(data.CommandId), Token);
			gameLogicEventsSource.Subscribe<CommandApprove>(data => QueueEnd(data.CommandId), Token);
			gameLogicEventsSource.Subscribe<CommandCancel>(data => QueueEnd(data.CommandId), Token);
			gameLogicEventsSource.Subscribe<TurnGame>(Clear, Token);
		}

		public override void Dispose()
		{
			base.Dispose();
			OnByQueueChanged = null;
			queueStarted.Clear();
		}

		private void QueueStart(string commandId)
		{
			if (queueStarted.Contains(commandId))
				return;
			
			queueStarted.Add(commandId);
			OnByQueueChanged?.Invoke();
		}

		private void QueueEnd(string commandId)
		{
			if (!queueStarted.Remove(commandId))
				return;
			
			OnByQueueChanged?.Invoke();
		}

		private void Clear()
		{
			queueStarted?.Clear();
			OnByQueueChanged?.Invoke();
		}
	}
}