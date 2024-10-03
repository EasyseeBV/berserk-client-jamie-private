using Newtonsoft.Json;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	public class SessionEndMessage : GameMessage
	{
		[JsonProperty("Message")]
		public SessionContextLightModel SessionContextLightModel { get; set; }
	}
}