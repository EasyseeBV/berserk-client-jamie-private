using Newtonsoft.Json;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	public class GameHandMessage : GameMessage
	{
		[JsonProperty("Message")]
		public SessionPlayerHandChangeModel SessionPlayerHandChangeModel;
	}
}