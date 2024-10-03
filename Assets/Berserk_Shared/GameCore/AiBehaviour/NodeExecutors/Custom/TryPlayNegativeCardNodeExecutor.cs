using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem.TargetSystem;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.TryPlayNegativeCard)]
	public class TryPlayNegativeCardNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var logicContext = SessionProcessor.LogicContext;
			var botState = SessionProcessor.Context.PlayerRepository.Get(x => x.RuntimeData.IsBot);
			var cardModel = node.Model as CardNodeModel;
			var possibleCards = SessionProcessor.Context.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InHand, botState.UserId, ObjectType.Spell);
			
			if (!string.IsNullOrEmpty(cardModel?.CardId))
				possibleCards = possibleCards.Where(c => c.Data.Id == cardModel.CardId);
			
			foreach (var card in possibleCards)
			{
				if (card.RuntimeData.Mana > botState.RuntimeData.Mana)
					continue;
				
				var effectDatas = SessionProcessor.Context.GameDatabase
					.GetEffects(card.RuntimeData.ImposingEffects)
					.ToArray();
				
				if (effectDatas.Any(effect => effect.TargetOwner == Owner.Self))
					continue;

				var manualEffect = effectDatas.FirstOrDefault(x => x.TargetMod == EffectTargetMod.PlayerPicked 
				                                                   && x.Phases.Contains(EffectPhase.AfterSpawn));
				var model = new CmdParamsModel(SessionProcessor.Context.Timer.RuntimeData.TimeHash, new PlayCardArgs())
				{
					ExecutorObjectId = card.RuntimeData.Id
				};
				
				if (manualEffect != null)
				{
					// local changes to select itself as the target for the effect
					var originalState = card.RuntimeData.State;
					card.RuntimeData.SetStateWithoutNotify(RuntimeState.InTable);

					var targets = logicContext.TargetConditionRepository
						.GetAllowedTargets(card, manualEffect)
						.Where(t => t.RuntimeData.OwnerUserId != botState.UserId);
					
					if (cardModel?.CardId == card.Data.Id && !string.IsNullOrEmpty(cardModel?.TargetCardId))
						targets = targets.Where(obj => obj.Data.Id == cardModel.TargetCardId);

					model.TargetObjectsIds.AddRange(targets
						.Select(x=> x.RuntimeData.Id)
						.TakeNRandom(manualEffect.GetMaxTargetCount()));
					
					card.RuntimeData.SetStateWithoutNotify(originalState);
				}

				if (SessionProcessor.Context.Timer.RuntimeData == null 
				    || SessionProcessor.Context.Timer.RuntimeData.State == TimerState.Ended)
					return BehaviourNodeState.Failure;
				
				if (manualEffect == null || model.TargetObjectsIds.Count > 0)
					logicContext.CommandController.Execute<PlayCardCmd>(botState.UserId, model, true);
			}

			return BehaviourNodeState.Success;
		}
	}
}