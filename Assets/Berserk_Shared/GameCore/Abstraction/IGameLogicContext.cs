using System;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.EffectSystem.TargetSystem;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IGameLogicContext : IDisposable
	{
		ICommandController CommandController { get; }
		ILogicQueueController LogicQueueController { get; }
		IGiveCardsService GiveCardsService { get; }
		IRuntimeStateController RuntimeStateController { get; }
		IRuntimeCardPositionController RuntimeCardPositionController { get; }
		IEffectExecutor EffectExecutor { get; }
		IEffectsFactory EffectsFactory { get; }
		IRuntimeFactory RuntimeFactory { get; }
		ITargetConditionRepository TargetConditionRepository { get; }
		IExecutorConditionRepository ExecutorConditionRepository { get; }
		IEffectPhaseProcessor EffectPhaseProcessor { get; }
		ITurnController TurnController { get; }
		ITargetResolver TargetResolver { get; }
	}
}
