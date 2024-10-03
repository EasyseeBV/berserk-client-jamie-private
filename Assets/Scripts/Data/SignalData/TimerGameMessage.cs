using System;
using Newtonsoft.Json;

namespace Vulcan.Data
{
	public class TimerGameMessage : GameMessage
	{
		[JsonProperty("Message")]
		public DateTime EntDateTime;
	}
}