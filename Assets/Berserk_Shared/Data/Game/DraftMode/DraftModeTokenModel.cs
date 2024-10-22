using System;
using System.Collections.Generic;

namespace Berserk.Shared.Data.Game.DraftMode
{
	public class DraftModeTokenModel
	{
		public DateTime AquisitionDate { get; set; }
		public DateTime? ExpirationDate { get; set; }
		public bool IsValid { get; set; }
		public virtual List<DraftModeBetModel> Bets { get; set; } = new();
		public virtual List<DraftModeStepModel> DraftSteps { get; set; } = new();
		public virtual List<DraftModeRoundModel> Rounds { get; set; } = new();
	}
}
