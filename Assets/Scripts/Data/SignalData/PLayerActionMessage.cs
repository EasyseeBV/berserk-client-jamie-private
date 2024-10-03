using Newtonsoft.Json;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	public class PLayerActionMessage : GameMessage
	{
		[JsonProperty("Message")]
		public PerformActionModel EntityStates;
	}
}