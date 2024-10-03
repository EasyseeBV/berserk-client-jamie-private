using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ClientPlatform
	{
		Unknown,
		Standalone,
		Android,
		IOS,
	}

}