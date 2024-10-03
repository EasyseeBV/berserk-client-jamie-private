namespace Berserk.Shared.SignalR.Lobby
{
	public class VulcaniteExpiredSignalDto
	{
		public string[] VulcaniteIds { get; }
		
		public VulcaniteExpiredSignalDto(string[] vulcaniteIds)
		{
			VulcaniteIds = vulcaniteIds;
		}
	}
}