using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
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
	[AiBehaviourExecutor(AiBehaviourNodeType.TryCreatureAttack)]
	public class TryCreatureAttackNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var cardModel = node.Model as CardNodeModel;
			var botState = SessionProcessor.Context.PlayerRepository.Get(x => x.RuntimeData.IsBot);
			var effectData = SessionProcessor.Context.GameDatabase.GetEffectConfig(EffectKeyword.GenericAttack.AsSystemEffectId());

			var possibleCards = SessionProcessor.Context.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, botState.UserId, ObjectType.Creature)
				.Where(c => SessionProcessor.LogicContext.ExecutorConditionRepository.IsCanAttack(c) && c.RuntimeData.Attack > 0);
			
			if (!string.IsNullOrEmpty(cardModel?.CardId))
				possibleCards = possibleCards.Where(c => c.Data.Id == cardModel.CardId);

			foreach (var card in possibleCards)
			{
				IEnumerable<IRuntimeGameObject> targets = SessionProcessor.LogicContext.TargetConditionRepository
					.GetAllowedTargets(card, effectData);
				
				if (cardModel?.CardId == card.Data.Id && !string.IsNullOrEmpty(cardModel?.TargetCardId))
					targets = targets.Where(obj => obj.Data.Id == cardModel.TargetCardId);

				var target = targets.FirstRandomOrDefault();
				if (target == null)
					continue;
				
				if (SessionProcessor.Context.Timer.RuntimeData == null 
				    || SessionProcessor.Context.Timer.RuntimeData.State == TimerState.Ended)
					return BehaviourNodeState.Failure;
				
				var model = new CmdParamsModel(SessionProcessor.Context.Timer.RuntimeData.TimeHash)
				{
					ExecutorObjectId = card.RuntimeData.Id,
					TargetObjectsIds = new List<int>{target.RuntimeData.Id}
				};
				
				SessionProcessor.LogicContext.CommandController.Execute<PerformAttackCmd>(botState.UserId, model, true);

			}
			return BehaviourNodeState.Success;
		}
	}
}