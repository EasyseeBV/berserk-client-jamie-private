using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Season
	{
		Origins,
		New_Beginnings,
		Winds_of_War,
		Wine_and_Woods,
		Frank_Frazetta,
		Seasons_of_Change,
		PlaceHolder_Season,
		Sands_of_Time,
		None,
	}
}