using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	public abstract class BuffReplacePercentEffect : BuffStatReplaceEffect
	{
		protected override IEnumerable<IStatModifier<int>> GetBuffModifiers(IntStat stat)
		{
			var currValue = GetBuffCurrentValue(stat);
			var maxValue = GetBuffMaximumValue(stat);
			if (currValue == 0 && maxValue == 0)
				yield break;

			yield return new ClampedPercentModifierInt()
				.SetPercentSource(GetPercentModifierSource())
				.SetMaxMax(GetRestrictionMaxMax(stat))
				.SetMinMax(GetRestrictionMinMax(stat))
				.SetMinCurrent(GetRestrictionMinCurrent(stat))
				.SetMaxCurrent(GetRestrictionMaxCurrent(stat))
				.SetModifierId(RuntimeArg.ModifierId)
				.SetPriority((int)EffectData.ExecutionOrder)
				.SetCurrModifier(currValue, 0, RevertCurrentWhenExpire)
				.SetMaxModifier(maxValue, 0, RevertMaximumWhenExpire);
		}

		protected virtual PercentModifierSource GetPercentModifierSource()
		{
			return EffectData.ValueMod switch
			{
				EffectValue.PercentFromCurrent => PercentModifierSource.Current,
				EffectValue.PercentFromMaximum => PercentModifierSource.Maximum,
				
				// TODO maybe it's not correct way
				EffectValue.Integer
					or EffectValue.AlliedCount
					or EffectValue.AlliedCountExceptSelf
					or EffectValue.AlliedHandCount
					or EffectValue.TableCardsCount
					or EffectValue.FreeSelfTableSpace
					or EffectValue.FreeOpponentTableSpace
					or EffectValue.EnemyCount
					or EffectValue.TargetCount
					or EffectValue.TurnWithoutCards
					or EffectValue.SelfLava
					or EffectValue.OpponentLava => PercentModifierSource.Current,
				_ => throw new NotImplementedException($"{nameof(GetPercentModifierSource)}: Unknown {nameof(EffectData.ValueMod)} : {EffectData.ValueMod}")
			};
		}
		
		protected override int GetBuffCurrentValue(IntStat stat)
		{
			return EffectData.Value;
		}

		protected override int GetBuffMaximumValue(IntStat stat)
		{
			return EffectData.Value;
		}
	}
}