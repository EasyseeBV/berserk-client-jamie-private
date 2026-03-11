using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Exceptions;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class PerformAbilityCmd : Command<PerformEffectArgs>
	{
		protected override void OnExecute()
		{
			if (GameContext.Timer.RuntimeData.State == TimerState.Ended)
				throw new Exception("Timer state is not Game");

			if (Executor.RuntimeData.OwnerUserId != RuntimePlayer.UserId)
				throw new Exception("Cant play other guy's card!");

			if (Executor.RuntimeData is not IRuntimeHeroData heroRuntimeData)
				throw new Exception("Executor should be hero!");

			if (Targets == null || Targets.Length == 0)
				throw new Exception("Any target not found!");
			
			if (Executor.RuntimeData.ImposingEffects.Count == 0)
				throw new Exception("Hero should have abilities to use!");
			
			if (string.IsNullOrEmpty(ArgsModel.EffectDataId))
				throw new Exception("Empty effectConfigId passed!");
			
			var abilityLavaCost = heroRuntimeData.AbilityManaCost;
			var abilityHpCost = heroRuntimeData.AbilityHpCost;
			
			if (RuntimePlayer.RuntimeData.Mana.Current < abilityLavaCost)
				throw new InvalidActionException(InvalidAction.ImNotAvailable, "Not enough lava to use ability!");

			if (heroRuntimeData.Hp.Current <= abilityHpCost)
				throw new InvalidActionException(InvalidAction.ImNotAvailable, "Not enough HP to use ability!");
			
			RuntimePlayer.SpendLava(abilityLavaCost);
			Executor.RuntimeData.Hp.Substract(abilityHpCost);
			
			heroRuntimeData.AbilityMoveCount.Substract(1);
			LogicContext.EffectExecutor.CreateAndExecuteEffectAuto(ArgsModel.EffectDataId, Executor, null, Targets);
		}

		protected override void OnCancel() // do not use Throw exception in OnCancel method
		{
			if (!Executed)
				return;
			
			if (Executor.RuntimeData is not IRuntimeHeroData heroRuntimeData)
			{
				DefaultSharedLogger.Error("Executor should be hero!");
				return;
			}

			heroRuntimeData.AbilityMoveCount.Set(heroRuntimeData.AbilityMoveCount.Previous);
		}
	}
}