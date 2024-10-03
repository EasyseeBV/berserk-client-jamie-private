using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.SwapStats)]
	public class SwapStatsEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			if (string.IsNullOrEmpty(EffectData.Meta))
				throw new ArgumentException("Meta data is empty, specify stat names separated by ',' ");
			
			var statIds = EffectData.Meta.Split(",");
			if (statIds == null || statIds.Length < 2 || statIds.Length % 2 != 0)
				throw new ArgumentException("To change the value of the stats, " + 
				                            "you need the number of stats to be a multiple of two.");

			for (var i = 0; i < statIds.Length; i+=2)
			{
				SwapStats(statIds[i], statIds[i+1]);
			}
		}

		private void SwapStats(string statNameA, string statNameB)
		{
			foreach (var target in GetExecutionTargets())
			{
				if (target.IsDead)
					continue;
				
				var statA = target.GetStatByName(statNameA);
				var statB = target.GetStatByName(statNameB);
				
				var statAClone = statA.Clone();
				var statBClone = statB.Clone();

				statA.Replace(statBClone, true);
				statB.Replace(statAClone, true);
				
				target.TryDie(Executor, EffectData.DamageType); // if after switching stats the target is dead
			}
		}
	}
}