using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Identity.Social
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ExternalProvider
	{
		Google,
		Facebook,
		Twitter,
		TikTok,
		Discord,
		Apple
	}
}