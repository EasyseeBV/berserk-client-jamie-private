using System;
using System.Linq;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers
{
	[EffectCondition(EffectConditionType.CompareTurn)]
	public class EffectConditionTurn : EffectConditionBase
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
				CompareOp.Less => GameContext.Timer.RuntimeData.Turn < value,
				CompareOp.More => GameContext.Timer.RuntimeData.Turn > value,
				CompareOp.Equal => GameContext.Timer.RuntimeData.Turn == value,
				CompareOp.LessOrEqual => GameContext.Timer.RuntimeData.Turn <= value,
				CompareOp.MoreOrEqual => GameContext.Timer.RuntimeData.Turn >= value,
				_ => throw new ArgumentOutOfRangeException($"Unknown {nameof(CompareOp)} : '{op}'")
			};
		}
	}
}