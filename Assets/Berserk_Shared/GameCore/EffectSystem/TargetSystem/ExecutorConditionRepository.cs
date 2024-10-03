using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.TargetSystem
{
	public interface IExecutorConditionRepository
	{
		bool IsCanAttack(IRuntimeGameObject executor);
	}

	public class ExecutorConditionRepository : IExecutorConditionRepository
	{
		private readonly IGameContext gameContext;

		public ExecutorConditionRepository(IGameContext gameContext)
		{
			this.gameContext = gameContext;
		}

		public bool IsCanAttack(IRuntimeGameObject executor)
		{
			return executor is {IsDead: false}
			       && executor.RuntimeData.OwnerUserId == gameContext.Timer.RuntimeData.OwnerId
			       && executor.RuntimeData.MoveCount > 0
			       && !executor.HasEffectsDisable()
			       && !executor.HasAppliedNonDisabledEffect(EffectKeyword.Petrify);
		}
		
		//todo #executorConditionRepository IsCanPlay
	}
	
}