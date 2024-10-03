using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Summon)]
	public class SummonEffect : KeywordEffect
	{
		public override bool CanExecute()
		{
			if (!base.CanExecute() || ValueModRounded() <= 0)
				return false;
			
			// predict available place on table for any side
			SetTargets(Targets
				.Where(target => !LogicContext.TargetConditionRepository.IsFullTable(target.RuntimeData.OwnerUserId))
				.ToArray());
			
			return base.CanExecute();
		}
		
		protected override void OnExecute()
		{
			if (string.IsNullOrEmpty(EffectData.Meta))
				throw new ArgumentNullException($"{nameof(EffectData.Meta)} must not be empty");

			var cardData = GameContext.GameDatabase.GetCard(EffectData.Meta);
			if (cardData == null || !cardData.Type.IsTableCard())
				throw new ArgumentNullException($"{nameof(EffectData.Meta)} must have contains table card id");
			
			foreach (var target in GetExecutionTargets())
			{
				var summonCount = ValueModRounded();
				for (var i = 0; i < summonCount; i++)
				{
					if (LogicContext.TargetConditionRepository.IsFullTable(target.RuntimeData.OwnerUserId))
						break;

					var runtimeCard = LogicContext.RuntimeFactory.CreateRuntimeCard(cardData.Id, target.RuntimeData.OwnerUserId);
					if (runtimeCard == null)
						throw new NullReferenceException($"The card could not be created: {cardData.ReflectionFormat()}");
					
					var playModel = new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash, new PlayCardArgs())
					{
						ExecutorObjectId = runtimeCard.RuntimeData.Id
					};
					LogicContext.CommandController.Execute<ApprovedPlayCardCmd>(target.RuntimeData.OwnerUserId, playModel, true);
				}
			}
		}
	}
}