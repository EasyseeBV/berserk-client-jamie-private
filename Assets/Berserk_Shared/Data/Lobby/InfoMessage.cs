using Newtonsoft.Json;

namespace Berserk.Shared.Data.Lobby
{
	public class InfoMessage
	{
		[JsonProperty("Message")]
		public OnlinePlayersModel OnlinePlayers;
	}
}