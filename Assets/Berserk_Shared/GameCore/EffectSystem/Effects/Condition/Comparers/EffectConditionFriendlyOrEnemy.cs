using System.Linq;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers
{
	[EffectCondition(EffectConditionType.FriendlyOrEnemy)]
	public class EffectConditionFriendlyOrEnemy : EffectConditionBase
	{
		public override bool Eval(params object[] args)
		{
			var targets = LinkedEffect.Targets;
			var target = targets[0];
			var isFriendly = target.RuntimeData.OwnerUserId == LinkedEffect.Executor.RuntimeData.OwnerUserId;
			var externalArgs = args.OfType<string>().FirstOrDefault()?.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
			if (externalArgs != null && externalArgs.Length > 0)
			{
				if (externalArgs[0] == "Enemy") return !isFriendly;
				if (externalArgs[0] == "Friendly") return isFriendly;
			}
			return isFriendly;
		}
	}
}