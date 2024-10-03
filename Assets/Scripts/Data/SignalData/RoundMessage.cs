using Newtonsoft.Json;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	public class RoundMessage : GameMessage
	{
		[JsonProperty("Message")]
		public NextRoundModel RoundModel;
	}
}