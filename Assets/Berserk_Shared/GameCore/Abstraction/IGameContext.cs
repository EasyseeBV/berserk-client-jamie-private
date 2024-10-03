using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IGameContext : IDisposable
	{
		IRuntimeTimer Timer { get; }
		IGameDatabase GameDatabase { get; }
		IGameRuntimePool GameRuntimePool { get; }
		IRuntimeIdGenerator RuntimeIdGenerator { get; }
		IRuntimeRandomGenerator RandomGenerator { get; }
		IPlayerRepository PlayerRepository { get; }
		IOrderGenerator OrderGenerator { get; }
		ISharedConfig SharedConfig { get; }
		ISharedTime SharedTime { get; }
		ISharedEventsSource SharedEventsSource { get; }
		IRuntimeContextData RuntimeData { get; }

		void Sync(IRuntimeContextData runtimeData);
		
		IRuntimeGameCard GetNextDeckCard(string userId);

		void Start();

		void End();
	}
}