using Vulcan.Data;

namespace ServerCore.Infrastructure.Models
{
	public class SessionPlayerLightModel
	{
		public string Id { get; set; }
		public bool IsControlledByAI { get; set; }
		public int PlayerIndex { get; set; }
		public PlayerConnectionStatus ConnectionStatus { get; set; }
	}
}