using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.LifeSteal)]
	public class LifeStealEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			for (var i = 0; i < Targets.Length; i++)
			{
				ApplyHealth(Executor.RuntimeData.Attack);
			}
		}

		private void ApplyHealth(int value)
		{
			if (Executor.RuntimeData.Hp + value > Executor.RuntimeData.Hp.TotalMax)
			{
				Executor.RuntimeData.Hp.ResetToMax();
				return;
			}
			Executor.RuntimeData.Hp.Add(value);
		}
	}
}