using System;
using RR.Game.TutorialSystemV2.Abstraction.Data;

namespace RR.Game.TutorialSystemV2.Data
{
	[Serializable]
	public class TutorialBlockData : ITutorialBlockData
	{
		public virtual string NextId { get; set; } = string.Empty;
		public virtual float DelayBeforeInvoke { get; set; } = 0f;
		public virtual float DelayAfterClose { get; set; } = 0f;
		public virtual bool PauseRequired { get; set; } = false;
		public virtual bool IsSyncRequired { get; set; } = true;
		public virtual string[] KeywordTriggers { get; set; } = Array.Empty<string>();
		public virtual string[] RequiredCompletedHints { get; set; } = Array.Empty<string>();
		public virtual string[] CompleteDependedHints  { get; set; } = Array.Empty<string>();
	}
}