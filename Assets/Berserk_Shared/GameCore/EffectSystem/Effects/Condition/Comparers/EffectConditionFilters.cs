using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;
using Berserk.Shared.GameCore.EffectSystem.TargetSystem;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers
{
	[EffectCondition(EffectConditionType.ParamFilters)]
	public class EffectConditionFilters : EffectConditionBase
	{
		public override bool Eval(params object[] args)
		{
			var externalArgs = args.OfType<string>()
				.FirstOrDefault()?
				.Split(",", StringSplitOptions.RemoveEmptyEntries);

			if (externalArgs == null || externalArgs.Length == 0)
				throw new ArgumentException("External args is null or empty");
			
			if (!Enum.TryParse<ParamFilter>(externalArgs[0], out var paramFilter))
				throw new ArgumentException($"External args isn't have {nameof(ParamFilter)} type");
			
			if (!int.TryParse(externalArgs[1], out var value))
				throw new ArgumentException($"External args isn't have Integer type as compare value");

			return LinkedEffect.Targets.ApplyParamFilter(LinkedEffect.Executor, paramFilter, value).Any();
		}
	}
}