using Newtonsoft.Json;

namespace Vulcan.Data
{

#region GameMessage
	
	public class GameMessage
	{
		[JsonProperty("Action")]
		public GameAction Action;

		[JsonProperty("SessionPlayerId")]
		public string SessionPlayerId;

		[JsonProperty("SessionId")]
		public string SessionId;

		[JsonProperty("UtcTimeStamp")]
		public long UtcTimeStamp;
	}

	#endregion

}