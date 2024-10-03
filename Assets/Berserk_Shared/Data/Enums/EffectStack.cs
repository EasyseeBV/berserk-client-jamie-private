using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectStack
	{
		DoNotStack,
		ReplaceLength,
		ReplaceMoreLength,
		ReplaceValue,
		ReplaceMoreValue,
		ReplaceMoreBoth,
		ReplaceBoth,
		AdditiveLength,
		AdditiveValue,
		AdditiveBoth,
		Custom,
	}
}