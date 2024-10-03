using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Faction // DO NOT REMOVE UNDERSCORE
	{
		None = 0,
		The_Order_of_Sagittarius,
		The_Wild_Way,
		The_Amazons,
		The_Horned_Lodge,
		Daemons_of_the_Dark,
		The_Shades,
		Company_of_Hoplites,
		Priests_of_Khemet,
		Servants_of_Vulcan,
		Iskandrian_Hegemony,
		
		/// <summary>
		/// Except flag for better experience in the config
		/// </summary>
		ExFlag = -1, 
	}
}