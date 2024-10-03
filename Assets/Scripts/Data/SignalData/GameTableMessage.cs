using Newtonsoft.Json;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	public class GameTableMessage : GameMessage
	{
		[JsonProperty("Message")]
		public PlayCardModel PlayCardModel;
	}
}