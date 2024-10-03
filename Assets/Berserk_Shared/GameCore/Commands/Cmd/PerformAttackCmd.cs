using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Exceptions;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class PerformAttackCmd : Command
	{
		private int moveCount;
		protected override void OnExecute()
		{
			Targets = Targets?.Where(x => !x.IsDead).ToArray();
			moveCount = Executor.RuntimeData.MoveCount;
			
			var effectId = EffectKeyword.GenericAttack.AsSystemEffectId();
			if (Targets is null or {Length: 0}
			    || Targets.All(target => !LogicContext.TargetConditionRepository.IsAllowedTarget(Executor, target, effectId)))
				throw new InvalidActionException(InvalidAction.NoAvailableTargets);

			if (!LogicContext.ExecutorConditionRepository.IsCanAttack(Executor))
				throw new InvalidActionException(InvalidAction.ImNotAvailable);
			
			LogicContext.EffectExecutor.CreateAndExecuteEffect(effectId, Executor, null, Targets);
			Executor.SpendMove();
		}
		
		protected override void OnCancel()
		{
			if (!Executed || Executor.IsDead)
				return;

			if (GameContext.Timer.RuntimeData.OwnerId != Executor.RuntimeData.OwnerUserId)
				return;
			
			Executor.RuntimeData.MoveCount.Set(moveCount);
		}
	}
}