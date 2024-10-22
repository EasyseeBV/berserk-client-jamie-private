using System;
using System.Collections.Generic;

namespace Berserk.Shared.Data.Game.DraftMode
{
	public class DraftModeStepModel
	{
		public string Id { get; set; }
		public DraftStepType Type { get; set; }
		public bool IsComplete { get; set; }
		public DateTime? CompletedAt { get; set; }
		public List<DraftModeStepElementModel> Elements { get; set; } = new();
	}
}
