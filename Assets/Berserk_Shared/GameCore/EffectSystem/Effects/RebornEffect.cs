using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Reborning)]
	public class RebornEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				if (LogicContext.TargetConditionRepository.IsFullTable(target.RuntimeData.OwnerUserId))
					return;
				
				var rebornedCard = LogicContext.RuntimeFactory.CreateRuntimeCard(target.Data.Id, target.RuntimeData.OwnerUserId);
				rebornedCard.RuntimeData.ImposingEffects.Remove(EffectData.Id);
				target.RuntimeData.ImposingEffects.Remove(EffectData.Id);
				rebornedCard.TurnToken(true);
				
				var playModel = new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash, new PlayCardArgs())
				{
					ExecutorObjectId = rebornedCard.RuntimeData.Id
				};
				LogicContext.CommandController.Execute<ApprovedPlayCardCmd>(Executor.RuntimeData.OwnerUserId, playModel, true);
			}
		}
	}
}