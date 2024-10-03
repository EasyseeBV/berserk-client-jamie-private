using System;
using System.Collections.Generic;

namespace RR.Game.TutorialSystem.Domain.Data
{
	[Serializable]
	public class ProgressData
	{
		public Dictionary<string, bool> CompletedTutorHints = new Dictionary<string, bool>();
	}
}