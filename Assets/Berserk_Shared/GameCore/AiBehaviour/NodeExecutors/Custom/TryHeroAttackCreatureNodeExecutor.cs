using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.EffectSystem.TargetSystem;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.TryHeroAttackCreature)]
	public class TryHeroAttackCreatureNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var botState = SessionProcessor.Context.PlayerRepository.Get(x => x.RuntimeData.IsBot);
			var hero = SessionProcessor.Context.GameRuntimePool.GetHeroByUserId(botState.UserId);

			if (!SessionProcessor.LogicContext.ExecutorConditionRepository.IsCanAttack(hero) || hero.RuntimeData.Attack == 0)
				return BehaviourNodeState.Success;

			var effectId = EffectKeyword.GenericAttack.AsSystemEffectId();
			var allowedTarget = SessionProcessor.LogicContext.TargetConditionRepository
				.GetAllowedTargets(hero, effectId)
				.GetObjectsFilterBy(type: ObjectType.Building | ObjectType.Creature, asQuery: true)
				.FirstRandomOrDefault();

			if (allowedTarget == null)
				return BehaviourNodeState.Success;

			if (SessionProcessor.Context.Timer.RuntimeData == null 
			    || SessionProcessor.Context.Timer.RuntimeData.State == TimerState.Ended)
				return BehaviourNodeState.Failure;

			var model = new CmdParamsModel(SessionProcessor.Context.Timer.RuntimeData.TimeHash)
			{
				ExecutorObjectId = hero.RuntimeData.Id,
				TargetObjectsIds = new List<int> { allowedTarget.RuntimeData.Id }
			};

			SessionProcessor.LogicContext.CommandController.Execute<PerformAttackCmd>(botState.UserId, model, true);
			return BehaviourNodeState.Success;
		}
	}
}