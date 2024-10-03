using Newtonsoft.Json;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	public class GameEntityMessage : GameMessage
	{
		[JsonProperty("Message")]
		public ModifyEntityModel ModifyEntityModel;
	}
}