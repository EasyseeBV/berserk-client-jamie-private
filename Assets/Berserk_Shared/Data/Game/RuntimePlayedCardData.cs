namespace Berserk.Shared.Data.Game
{
	public readonly struct RuntimePlayedCardData
	{
		public string CardId { get; }
		public int RuntimeId { get; }
		public int Round { get; }
		public int Turn { get; }
		
		public RuntimePlayedCardData(string cardId, int runtimeId, int round, int turn)
		{
			CardId = cardId;
			RuntimeId = runtimeId;
			Round = round;
			Turn = turn;
		}
	}
}