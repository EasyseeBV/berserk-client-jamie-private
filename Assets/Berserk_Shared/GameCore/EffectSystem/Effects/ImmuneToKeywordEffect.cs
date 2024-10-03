using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.ImmuneToKeyword)]
	public class ImmuneToKeywordEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				foreach (var immuneKeyword in GetImmuneKeywords(target))
				{
					RevealAppliedEffect(immuneKeyword.Keyword, target);
					if (target.RuntimeData.ImmuneToKeywords.Contains(immuneKeyword))
						continue;

					target.RuntimeData.ImmuneToKeywords.Add(immuneKeyword);
					LogicContext.LogicQueueController.Add(new AddImmuneToKeyword(immuneKeyword, target.RuntimeData.Id), target.GetAccessibleReceiver());
				}
			}
		}

		protected override void OnExpire()
		{
			foreach (var target in GetExecutionTargets())
			{
				foreach (var immuneKeyword in GetImmuneKeywords(target))
				{
					if (target.RuntimeData.ImmuneToKeywords.Remove(immuneKeyword))
						LogicContext.LogicQueueController.Add(new DeleteImmuneToKeyword(immuneKeyword, target.RuntimeData.Id), target.GetAccessibleReceiver());
				}
			}

			base.OnExpire();
		}

		protected virtual void RevealAppliedEffect(string keyword, IRuntimeGameObject target)
		{
			target?.RemoveAppliedEffect(keyword);
		}
		
		protected virtual IEnumerable<ImmuneKeyword> GetImmuneKeywords(IRuntimeGameObject target)
		{
			return EffectData.GetConfigIdsFromMeta().Select(keyword => new ImmuneKeyword(keyword, RuntimeData.Id));
		}
	}
}