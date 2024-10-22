using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Game.DraftMode
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DraftStepType
	{
		Hero = 0,
		Card = 2
	}
}