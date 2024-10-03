using System;

namespace RR.Game.TutorialSystemV2.Data
{
	[Serializable]
	public class TutorialBlock
	{
		public virtual string Id { get; set; } = string.Empty;
		public virtual TutorialBlockData Data { get; set; } = new();
		public virtual TutorialPopupData Popup { get; set; } = new();
		public virtual TutorialPointerData Pointer { get; set; } = new();
		public virtual TutorialConditionData[] Conditions { get; set; } = Array.Empty<TutorialConditionData>();
		public virtual TutorialUnmaskData[] Unmasks { get; set; } = Array.Empty<TutorialUnmaskData>();
	}
}