using System;
using Berserk.Shared.GameCore.LogicContext;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore
{
	public class ClampedModifierInt : SimpleModifierInt
	{
		[JsonProperty] protected int? MaxMax;
		[JsonProperty] protected int? MinMax;
		[JsonProperty] protected int? MaxCur;
		[JsonProperty] protected int? MinCur;

		#region Setters

		public ClampedModifierInt SetMaxMax(int? maxMax)
		{
			MaxMax = maxMax;
			return this;
		}

		public ClampedModifierInt SetMinMax(int? minMax)
		{
			MinMax = minMax;
			return this;
		}

		public ClampedModifierInt SetMaxCurrent(int? maxCur)
		{
			MaxCur = maxCur;
			return this;
		}

		public ClampedModifierInt SetMinCurrent(int? minCur)
		{
			MinCur = minCur;
			return this;
		}

		#endregion
		
		public override void ApplyMaximum(IStatModifiable<int> stat)
		{
			MaximumApplied = Apply(stat.AddModMax, Clamp(ModifierMaximum, MinMax, MaxMax, stat.TotalMax));
		}

		public override void ApplyCurrent(IStatModifiable<int> stat)
		{
			var currToApply = ModifierCurrrent - CurrentApplied;
			if (currToApply != 0)
				CurrentApplied += Apply(value => stat.Add(value, false), Clamp(currToApply, MinCur, MaxCur, stat.Current));
		}

		public override void Expire(IStatModifiable<int> stat)
		{
			if (MaximumApplied != 0 && RemoveMaxWhenExpire)
			{
				Apply(stat.AddModMax, Clamp(Sign(MaximumApplied, true), MinMax, MaxMax, stat.TotalMax));
				MaximumApplied = 0;
			}

			if (CurrentApplied != 0 && RemoveCurrWhenExpire)
			{
				Apply(value => stat.Add(value, false), Clamp(Sign(CurrentApplied, true), MinCur, MaxCur, stat.Current));
				CurrentApplied = 0;
			}
		}

		private static int Clamp(int modifierValue, int? min, int? max, int statValue)
		{
			TryRestrict(statValue, modifierValue, min, total => total>=min, out var result);
			TryRestrict(statValue, result ?? modifierValue, max, total => total<=max, out result);

			return result ?? modifierValue;
		}

		private static void TryRestrict(int statValue, int modifierValue, int? limit, Func<int, bool> compareFunc, out int? result)
		{
			if (compareFunc == null)
			{
				DefaultSharedLogger.Error($"[{nameof(ClampedModifierInt)}] Compare function is missing");
				result = null;
				return;
			}
			
			var total = statValue + modifierValue;
			if (!limit.HasValue || modifierValue == 0 || compareFunc.Invoke(total))
			{
				result = modifierValue;
				return;
			}
			
			var except = total - limit.Value;
			var min = Math.Min(0, modifierValue);
			var max = Math.Max(0, modifierValue);
			result = Math.Clamp(modifierValue - except, min , max);
		}
	}
}