using Berserk.Shared.SignalR.Enums;

namespace Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching
{
	public class AutoMatchActionDeniedModel : AutoMatchBaseModel
	{
		public LobbyAutoMatchingAction Action { get; set; }
		public string Reason { get; set; }
	}
}