namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangedPlayerStatistic : LogicEvent
	{
		public string UserId { get; }
		public int EloDelta { get; }
		public int EloTotal { get; }

		public ChangedPlayerStatistic(string userId, int eloDelta, int eloTotal)
		{
			UserId = userId;
			EloDelta = eloDelta;
			EloTotal = eloTotal;
		}
	}
}