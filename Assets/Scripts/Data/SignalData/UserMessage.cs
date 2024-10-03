using Newtonsoft.Json;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	public class UserMessage : GameMessage
	{
		[JsonProperty("Message")]
		public SessionPlayerLightModel PlayerModel;
	}
}