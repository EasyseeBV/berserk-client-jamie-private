using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ServerCore.Infrastructure.Models
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum PlayerConnectionStatus : byte
	{
		Disconnected = 0,
		Connected = 1
	}
}