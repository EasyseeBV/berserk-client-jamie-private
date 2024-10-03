using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.TryCreatureAttackOpponent)]
	public class TryCreatureAttackOpponentNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var botState = SessionProcessor.Context.PlayerRepository.Get(x => x.RuntimeData.IsBot);
			var opponent = SessionProcessor.Context.PlayerRepository.GetOpposite(botState.UserId);
			var opponentHero = SessionProcessor.Context.GameRuntimePool.GetHeroByUserId(opponent.UserId);
			var effectData = SessionProcessor.Context.GameDatabase.GetEffectConfig(EffectKeyword.GenericAttack.AsSystemEffectId());
			var cardModel = node.Model as CardNodeModel;
			
			var possibleCards = SessionProcessor.Context.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, botState.UserId, ObjectType.Creature)
				.Where(c => SessionProcessor.LogicContext.ExecutorConditionRepository.IsCanAttack(c) && c.RuntimeData.Attack > 0);

			if (!string.IsNullOrEmpty(cardModel?.CardId))
				possibleCards = possibleCards.Where(c => c.Data.Id == cardModel.CardId);

			foreach (var card in possibleCards)
			{
				if (SessionProcessor.Context.Timer.RuntimeData == null
				    || SessionProcessor.Context.Timer.RuntimeData.State == TimerState.Ended)
					return BehaviourNodeState.Failure;

				if (!SessionProcessor.LogicContext.TargetConditionRepository.IsAllowedTarget(card, opponentHero, effectData))
					continue;

				var model = new CmdParamsModel(SessionProcessor.Context.Timer.RuntimeData.TimeHash)
				{
					ExecutorObjectId = card.RuntimeData.Id,
					TargetObjectsIds = new List<int> {opponentHero.RuntimeData.Id}
				};

				SessionProcessor.LogicContext.CommandController.Execute<PerformAttackCmd>(botState.UserId, model, true);
			}

			return BehaviourNodeState.Success;
		}
	}
}