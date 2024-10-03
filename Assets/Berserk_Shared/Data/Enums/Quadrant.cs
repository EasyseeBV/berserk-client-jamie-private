using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Quadrant
	{
		None = 0,
		Arcadia,
		Hades,
		Notus,
		Boreas,
		Neutral,
		Vulcan_City,
		
		/// <summary>
		/// Except flag for better experience in the config
		/// </summary>
		ExFlag = -1,
	}
}