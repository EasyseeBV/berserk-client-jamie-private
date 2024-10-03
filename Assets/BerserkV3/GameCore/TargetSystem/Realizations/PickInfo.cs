using System.Collections.Generic;
using BerserkV3.GameCore.UI;
using RR.Core.DebugSystem;

namespace BerserkV3.GameCore.TargetSystem
{
	public class PickInfo
	{
		private readonly int admissibleCount;
		public IRuntimeObjectView From { get; }
		public IList<IRuntimeObjectView> Targets { get; }
		public string EffectId { get; }
		public int AdmissibleCount { get; }
		
		public PickInfo(IRuntimeObjectView from, string effectId, int admissibleCount)
		{
			AdmissibleCount = admissibleCount;
			From = from;
			EffectId = effectId;
			Targets = new List<IRuntimeObjectView>();
		}
		
		public void Assign(IRuntimeObjectView target)
		{
			if (target == null)
			{
				RRLogger.Error($"Cannot {nameof(Assign)} to null. Use {nameof(Reset)} if necessary");
				return;
			}

			Targets.Add(target);
		}
		
		public void Reset()
		{
			Targets.Clear();
		}
	}
}