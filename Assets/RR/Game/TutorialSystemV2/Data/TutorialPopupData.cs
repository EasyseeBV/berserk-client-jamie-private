using System;
using RR.Game.TutorialSystemV2.Abstraction.Data;

namespace RR.Game.TutorialSystemV2.Data
{
	[Serializable]
	public class TutorialPopupData : ITutorialPopupData
	{
		public virtual string Title { get; set; } = string.Empty;
		public virtual string Text { get; set; } = string.Empty;
		public virtual string ContinueButtonText { get; set; } = string.Empty;
		public virtual TutorialVector2 Position { get; set; }
		public virtual TutorialVector2 SizeDelta { get; set; }
		public virtual float DelayBeforShow { get; set; } = 0f;
		public virtual bool ArrowRequired { get; set; } = false;
		public virtual bool ContinueButton { get; set; } = true;
		public virtual bool BackButton { get; set; } = true;
		public virtual bool BackHinder { get; set; } = true;
		public virtual bool AutoSize { get; set; } = true;
		public virtual bool Enabled { get; set; } = true;
	}
}