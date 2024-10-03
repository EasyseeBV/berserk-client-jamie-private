using System;
using RR.Game.TutorialSystemV2.Abstraction.Data;

namespace RR.Game.TutorialSystemV2.Data
{
	[Serializable]
	public class TutorialPointerData : ITutorialPointerData
	{
		public virtual TutorialVector3 StartPosition { get; set; }
		public virtual TutorialVector3 EndPosition { get; set; }
	}
}