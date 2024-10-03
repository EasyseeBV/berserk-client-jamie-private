using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.LogicContext
{
	public readonly struct StartGameEvent : ISharedEvent
	{
		public DateTime? StartTime { get; }

		public StartGameEvent(DateTime? startTime)
		{
			StartTime = startTime;
		}
	}

	public class GameContext : IGameContext
	{
		public IRuntimeTimer Timer { get; }
		public ISharedConfig SharedConfig { get; }
		public ISharedTime SharedTime { get; }
		public IGameDatabase GameDatabase { get; }
		public IGameRuntimePool GameRuntimePool { get; }
		public IRuntimeIdGenerator RuntimeIdGenerator { get; }
		public IRuntimeRandomGenerator RandomGenerator { get; }
		public IPlayerRepository PlayerRepository { get; }
		public IOrderGenerator OrderGenerator { get; }
		public ISharedEventsSource SharedEventsSource { get; }
		public IRuntimeContextData RuntimeData { get; private set; }

		public GameContext(
			IGameDatabase gameDatabase,
			ISharedConfig sharedConfig,
			IGameRuntimePool gameRuntimePool,
			IRuntimeIdGenerator runtimeIdGenerator,
			IRuntimeRandomGenerator randomGenerator,
			IPlayerRepository playerRepository,
			IOrderGenerator orderGenerator,
			IRuntimeTimer runtimeTimer,
			ISharedTime sharedTime,
			ISharedEventsSource sharedEventsSource)
		{
			SharedTime = sharedTime;
			SharedEventsSource = sharedEventsSource;
			GameDatabase = gameDatabase;
			SharedConfig = sharedConfig;
			GameRuntimePool = gameRuntimePool;
			RuntimeIdGenerator = runtimeIdGenerator;
			PlayerRepository = playerRepository;
			RandomGenerator = randomGenerator;
			OrderGenerator = orderGenerator;
			Timer = runtimeTimer;
		}

		public void Sync(IRuntimeContextData runtimeData)
		{
			RuntimeData = runtimeData;
		}

		public IRuntimeGameCard GetNextDeckCard(string userId)
		{
			return GameRuntimePool.GetCardsFilterBy(RuntimeState.InDeck, userId, asQuery: true).FirstOrDefault();
		}

		public void Start()
		{
			RuntimeData.StartTime = SharedTime.Current;
			SharedEventsSource.Publish(new StartGameEvent(RuntimeData.StartTime));
		}

		public void End()
		{
			RuntimeData.EndTime = SharedTime.Current;
		}

		public void Dispose()
		{
			GameRuntimePool?.Clear();
			PlayerRepository?.Clear();
			SharedEventsSource?.Dispose();
		}
	}
}