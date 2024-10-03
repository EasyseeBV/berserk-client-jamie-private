using System;
using System.Linq;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers
{
	[EffectCondition(EffectConditionType.TargetsCount)]
	public class EffectConditionTargets : EffectConditionBase
	{
		public override bool Eval(params object[] args)
		{
			var externalArgs = args.OfType<string>()
				.FirstOrDefault()?
				.Split(",", StringSplitOptions.RemoveEmptyEntries);
			
			if (externalArgs == null || externalArgs.Length == 0)
				throw new ArgumentException("External args is null or empty");
			
			if (!Enum.TryParse<CompareOp>(externalArgs[0], out var op))
				throw new ArgumentException($"External args isn't have {nameof(CompareOp)} type");
			
			if (!int.TryParse(externalArgs[1], out var value))
				throw new ArgumentException($"External args isn't have Integer type as compare value");
			
			return op switch 
			{
				CompareOp.None => true,
				CompareOp.Less => LinkedEffect.Targets.Length < value,
				CompareOp.More => LinkedEffect.Targets.Length > value,
				CompareOp.Equal => LinkedEffect.Targets.Length == value,
				CompareOp.LessOrEqual => LinkedEffect.Targets.Length <= value,
				CompareOp.MoreOrEqual => LinkedEffect.Targets.Length >= value,
				_ => throw new ArgumentOutOfRangeException($"Unknown {nameof(CompareOp)} : '{op}'")
			};
		}
	}
}