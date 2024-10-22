using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.GameCore
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum PercentModifierSource
	{
		Current,
		Maximum
	}
	
	public class ClampedPercentModifierInt : ClampedModifierInt
	{
		[JsonProperty] public PercentModifierSource PercentSource { get; private set; }

		public ClampedPercentModifierInt SetPercentSource(PercentModifierSource value)
		{
			PercentSource = value;
			return this;
		}

		public override void Apply(IStatModifiable<int> stat)
		{
			var valueSource = GetPercentSource(stat);
			var valuePercentMax = CalcValuePercent(valueSource, ModifierMaximum);
			var valuePercentCurr = CalcValuePercent(valueSource, ModifierCurrent);
			
			MaximumApplied = Apply(stat.AddModMax, Clamp(valuePercentMax, MinMax, MaxMax, stat.TotalMax));
			CurrentApplied = Apply(stat.AddModCurrent, Clamp(valuePercentCurr, MinMax, MaxMax, stat.TotalCurrent));
		}

		private int GetPercentSource(IStatModifiable<int> stat)
		{
			return PercentSource switch
			{
				PercentModifierSource.Current => stat.TotalCurrent,
				PercentModifierSource.Maximum => stat.TotalMax,
				_ => throw new NotImplementedException($"{nameof(GetPercentSource)}: Unknown {nameof(PercentSource)} : {PercentSource}")
			};
		}

		private static int CalcValuePercent(int value, int percent)
		{
			var percentSign = Math.Sign(percent);
			var absPercent = Math.Abs(percent);
			var absStatValue = Math.Abs(value);
			
			var absValuePercent = (absPercent * absStatValue) / 100f;
			var roundedAbsValuePercent = percentSign <= 0 
				? (int)Math.Ceiling(absValuePercent) 
				: (int)Math.Floor(absValuePercent);

			return roundedAbsValuePercent * percentSign;
		}
	}
}