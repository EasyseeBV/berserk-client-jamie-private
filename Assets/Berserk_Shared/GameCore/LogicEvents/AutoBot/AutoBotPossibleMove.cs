namespace Berserk.Shared.GameCore.LogicEvents.AutoBot
{
	public class AutoBotPossibleMove : LogicEvent
	{
		public string TurnOwnerId { get; }
		
		public AutoBotPossibleMove(string turnOwnerId)
		{
			TurnOwnerId = turnOwnerId;
		}
	}
}