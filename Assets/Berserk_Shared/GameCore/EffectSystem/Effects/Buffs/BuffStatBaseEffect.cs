using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	public abstract class BuffStatBaseEffect : KeywordEffect
	{
		protected virtual bool RevertCurrentWhenExpire => false;
		protected virtual bool RevertMaximumWhenExpire => true;
		protected BuffStatEffectArg RuntimeArg => RuntimeData.GetRuntimeArg<BuffStatEffectArg>();

		public override void Create()
		{
			base.Create();
			RuntimeData.RuntimeArgs.Add(new BuffStatEffectArg {ModifierId = RuntimeData.Id.ToString()});
		}

		protected void OverrideModifierId(string id)
		{
			RuntimeArg.ModifierId = id;
		}
		
		protected override IRuntimeEffect OnStacked(IRuntimeEffect other)
		{
			if (other is BuffStatBaseEffect baseEffect)
				baseEffect.OverrideModifierId(RuntimeArg.ModifierId);
			
			return base.OnStacked(other);
		}

		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				foreach (var stat in GetAffectedStats(target))
				{
					var modifiers = stat.GetModifiers(RuntimeArg.ModifierId).ToArray();
					
					if (modifiers.Length == 0)
					{
						BuffTarget(target, stat);
						continue;
					}
					
					StackModifiers(stat, modifiers, out var modifier, out var stack);
					ApplyToModifier(stat, modifier, stack);
				}
			}
		}

		protected virtual void StackModifiers(
			IntStat stat, 
			IStatModifier<int>[] modifiers,
			out IStatModifier<int> modifier,  
			out ModifierStack<int> stack)
		{
			modifier = modifiers[0];
			stack = modifier.GetDataStack();
					
			for (var i = 1; i < modifiers.Length; i++)
			{
				var modToRemove = modifiers[i];
				stack = modToRemove.GetDataStack(stack);
				stat.RemoveModifier(modToRemove, false);
			}
		}

		protected virtual void ApplyToModifier(IntStat stat, IStatModifier<int> modifier, ModifierStack<int> stack)
		{
			modifier.SetCurrModifier(stack.ModifierCurrentTotal + GetBuffCurrentValue(stat), stack.CurrentAppliedTotal, RevertCurrentWhenExpire)
				.SetMaxModifier(stack.ModifierMaximumTotal + GetBuffMaximumValue(stat), stack.MaximumAppliedTotal, RevertMaximumWhenExpire);
			stat.NotifyChanges(true);
		}

		protected virtual void BuffTarget(IRuntimeGameObject target, IntStat stat)
		{
			var modifiers = GetBuffModifiers(stat);
			if (modifiers == null)
				return;
			
			if (!RuntimeData.AppliedIds.Contains(target.RuntimeData.Id))
				RuntimeData.AppliedIds.Add(target.RuntimeData.Id);
			
			foreach (var modifier in modifiers)
				stat.AddModifier(modifier, false);
			
			stat.NotifyChanges();
		}

		protected override void OnExpire()
		{
			var targets = GameContext.GameRuntimePool.GetMany(RuntimeData.AppliedIds.Distinct());
			foreach (var target in targets)
				ResetBuff(target);
			
			base.OnExpire();
		}

		protected virtual void ResetBuff(IRuntimeGameObject target, bool notify = true)
		{
			foreach (var stat in GetAffectedStats(target))
				stat.RemoveModifiers(RuntimeArg.ModifierId, notify);
		}

		protected virtual IEnumerable<IStatModifier<int>> GetBuffModifiers(IntStat stat)
		{
			var currValue = GetBuffCurrentValue(stat);
			var maxValue = GetBuffMaximumValue(stat);
			if (currValue == 0 && maxValue == 0)
				yield break;
			
			yield return new ClampedModifierInt()
				.SetMaxMax(GetRestrictionMaxMax(stat))
				.SetMinMax(GetRestrictionMinMax(stat))
				.SetMinCurrent(GetRestrictionMinCurrent(stat))
				.SetMaxCurrent(GetRestrictionMaxCurrent(stat))
				.SetModifierId(RuntimeArg.ModifierId)
				.SetPriority((int)EffectData.ExecutionOrder)
				.SetCurrModifier(currValue, 0, RevertCurrentWhenExpire)
				.SetMaxModifier(maxValue, 0, RevertMaximumWhenExpire);
		}

		protected virtual int GetBuffCurrentValue(IntStat stat)
		{
			return ValueModRounded(stat);
		}

		protected virtual int GetBuffMaximumValue(IntStat stat)
		{
			return ValueModRounded(stat);
		}
		
		protected abstract IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target);

		#region Restrictions
		/// <summary>
		/// The modifier value will be limited to the allowed minimum by the maximum stat value.
		/// </summary>
		/// <param name="stat">target stat</param>
		/// <returns>restricted value of stat, if null will skipp restrictions</returns>
		protected virtual int? GetRestrictionMinMax(IntStat stat)
		{
			return 0;
		}

		/// <summary>
		/// The modifier value will be limited to the allowed maximum by the maximum stat value.
		/// </summary>
		/// <param name="stat">target stat</param>
		/// <returns>restricted value of stat, if null will skipp restrictions</returns>
		protected virtual int? GetRestrictionMaxMax(IntStat stat)
		{
			return null;
		}

		/// <summary>
		/// The modifier value will be limited to the allowed minimum by the current stat value.
		/// </summary>
		/// <param name="stat">target stat</param>
		/// <returns>restricted value of stat, if null will skipp restrictions</returns>
		protected virtual int? GetRestrictionMinCurrent(IntStat stat)
		{
			return null;
		}

		/// <summary>
		/// The modifier value will be limited to the allowed maximum by the current stat value.
		/// </summary>
		/// <param name="stat">target stat</param>
		/// <returns>restricted value of stat, if null will skipp restrictions</returns>
		protected virtual int? GetRestrictionMaxCurrent(IntStat stat)
		{
			return null;
		}
		#endregion
	}
}