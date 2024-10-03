using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum PracticeMode
	{
		None = 0,
		Normal,
		Hard,
		Tutorial
	}
}