using System.Linq;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{ 
	public abstract class BuffStatReplaceEffect : BuffStatBaseEffect
	{
		protected override bool RevertCurrentWhenExpire => true;

		protected override IRuntimeEffect OnStacked(IRuntimeEffect other)
		{
			base.OnStacked(other);
			return this;
		}
		
		public override bool CanExecute()
		{
			return EffectData.MinTargetCount <= 0 || GetExecutionTargets().Length >= EffectData.MinTargetCount;
		}
		
		public override IRuntimeGameObject[] GetExecutionTargets()
		{
			return base.GetExecutionTargets().Where(target => 
			{
				return GetAffectedStats(target).Any(stat =>
				{
					var totalBuff = stat.GetModifiers(RuntimeArg.ModifierId).Sum(m => m.ModifierMaximum);
					return GetBuffMaximumValue(stat) - totalBuff != 0;
				}); 
			}).ToArray();
		}

		protected override void ApplyToModifier(IntStat stat, IStatModifier<int> modifier, ModifierStack<int> stack)
		{
			modifier.SetCurrModifier(GetBuffCurrentValue(stat), stack.CurrentAppliedTotal, RevertCurrentWhenExpire)
				.SetMaxModifier(GetBuffMaximumValue(stat), stack.MaximumAppliedTotal, RevertMaximumWhenExpire);
			stat.NotifyChanges(true);
		}
	}
}