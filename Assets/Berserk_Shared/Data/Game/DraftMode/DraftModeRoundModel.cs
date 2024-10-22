using System;

namespace Berserk.Shared.Data.Game.DraftMode
{
	public class DraftModeRoundModel
	{
		public string Id { get; set; }
		public int RoundIndex { get; set; }
		public int CoinsFromSystem { get; set; }
		public string OpponentUsername { get; set; }
		public bool IsComplete { get; set; }
		public bool IsWin { get; set; }
		public bool IsCompletionAcknowledgedByUser { get; set; }
		public DateTime? CompletionDate { get; set; }

		public virtual DraftModeBetModel OwnerBet { get; set; }
		public virtual DraftModeBetModel OpponentBet { get; set; }
	}
}
