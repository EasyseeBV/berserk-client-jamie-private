using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Resurrect)]
	public class ResurrectEffect : KeywordEffect
	{
		protected virtual bool RemoveIresurrectables => true;
		protected override void OnExecute()
		{
			foreach (var target in Targets.OfType<IRuntimeGameCard>())
				Resurrect(target, false);
		}

		/// <summary>
		/// Resurrect target card
		/// </summary>
		/// <returns>False when resurrect impossible</returns>
		protected bool Resurrect(IRuntimeGameCard target, bool ignoreTableSpace)
		{
			if (!ignoreTableSpace && LogicContext.TargetConditionRepository.IsFullTable(target.RuntimeData.OwnerUserId))
				return false;
			
			ResetBeforeResurrect(target);
			OnResurrect(target);
			target.ChangeEffectPhase(EffectPhase.AfterResurrection, damageType: EffectData.DamageType);
			return true;
		}

		protected virtual void ResetBeforeResurrect(IRuntimeGameCard target)
		{
			var imposingEffectIds = RemoveIresurrectables
				? GameContext.GameDatabase
					.GetEffects(target.Data.EffectsIds.Except(target.Data.IresurrectableIds))
					.Select(x => x.Id)
					.ToList()
				: target.Data.EffectsIds.ToList();
			
			if (target.RuntimeData.ImposingEffects.Count != imposingEffectIds.Count
			    || !target.RuntimeData.ImposingEffects.All(imposingEffectIds.Contains))
			{
				target.RuntimeData.ImposingEffects = imposingEffectIds;
				var setEffects = new SetImposingEffects(target.RuntimeData.Id, target.RuntimeData.ImposingEffects.ToArray());
				LogicContext.LogicQueueController.Add(setEffects, target.GetAccessibleReceiver());
			}
			target.ResetEffects();
			target.ResetStatsToDefault();
		}

		protected virtual void OnResurrect(IRuntimeGameCard target)
		{			
			var param = new PlayCardArgs();
			var playModel = new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash, param)
			{
				ExecutorObjectId = target.RuntimeData.Id
			};
			LogicContext.CommandController.Execute<ApprovedPlayCardCmd>(Executor.RuntimeData.OwnerUserId, playModel, true);
		}
	}
}
