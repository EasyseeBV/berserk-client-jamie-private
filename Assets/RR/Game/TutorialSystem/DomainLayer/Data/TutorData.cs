using System;
using System.Collections.Generic;

namespace RR.Game.TutorialSystem.Domain.Data
{
	[Serializable]
	public class TutorData
	{
		public List<TutorHintData> TutorHints { get; set; } = new();
	}
}