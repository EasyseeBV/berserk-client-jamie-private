using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents.AutoBot;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Mulligan)]
	public class MulliganEffect : KeywordEffect
	{
		public override void Create()
		{
			base.Create();
			RuntimeData.AccessLevel |= AccessLevel.Self;
			SetOrderedTargets(LogicContext.GiveCardsService.GiveStartingCardsToPlayer(Executor.RuntimeData.OwnerUserId));
			LogicContext.LogicQueueController.Add(new MulliganStart(), Executor.RuntimeData.OwnerUserId);
		}

		public override bool CanExecute()
		{
			return !GameContext.PlayerRepository.Get(Executor.RuntimeData.OwnerUserId).RuntimeData.IsFinishedMulligan;
		}

		protected override void OnExecute()
		{
			SetMulliganPassed();
			SetOrderedTargets(LogicContext.GiveCardsService.ReplaceMulliganCards(Executor.RuntimeData.OwnerUserId,
				Targets.Select(x => x.RuntimeData.Id).ToArray()));

			if (GameContext.PlayerRepository.All(p => p.RuntimeData.IsFinishedMulligan || p.RuntimeData.IsBot))
			{
				if (GameContext.RuntimeData.MatchMode == MatchMode.Tutorial)
					GameContext.Timer.Unpause(false);
				
				GameContext.Timer.SetState(TimerState.Ready);
			}
		}

		public override void OnDeleted()
		{
			base.OnDeleted();
			SetMulliganPassed();

			foreach (var card in GameContext.GameRuntimePool.GetCardsFilterBy(RuntimeState.InChoose, Executor.RuntimeData.OwnerUserId))
				card.ReturnToHand();
			
			LogicContext.GiveCardsService.UpdateNextDeckCard(Executor.RuntimeData.OwnerUserId);
			LogicContext.LogicQueueController.Add(new MulliganFinished(), Executor.RuntimeData.OwnerUserId);
			Executor.ChangeEffectPhase(EffectPhase.GameStart);
		}

		private void SetMulliganPassed()
		{
			var runtimePlayer = GameContext.PlayerRepository.Get(Executor.RuntimeData.OwnerUserId);
			if (runtimePlayer.RuntimeData.IsFinishedMulligan)
				return;
			runtimePlayer.SetFinishedMulligan(true);
		}

		private void SetOrderedTargets(IEnumerable<IRuntimeGameObject> targets)
		{
			SetTargets(targets.OrderBy(t=> t is IRuntimeGameCard gameCard 
				? gameCard.RuntimeData.RelativePositionX 
				: int.MaxValue).ToArray());
		}
	}
}