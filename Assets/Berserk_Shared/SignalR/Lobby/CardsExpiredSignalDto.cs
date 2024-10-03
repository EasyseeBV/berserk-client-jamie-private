namespace Berserk.Shared.SignalR.Lobby
{
	public class CardsExpiredSignalDto
	{
		public string[] CardIds { get; }
		
		public CardsExpiredSignalDto(string[] cardIds)
		{
			CardIds = cardIds;
		}
	}
}