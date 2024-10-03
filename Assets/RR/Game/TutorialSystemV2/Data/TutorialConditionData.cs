using System;
using RR.Game.TutorialSystemV2.Abstraction.Data;

namespace RR.Game.TutorialSystemV2.Data
{
	[Serializable]
	public class TutorialConditionData : ITutorialConditionData
	{
		public virtual string Id { get; set; } = string.Empty;
		public virtual string Meta { get; set; } = string.Empty;
	}
}