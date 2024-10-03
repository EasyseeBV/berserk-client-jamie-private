using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DamageType
	{
		None,
		Direct,
		CounterAttack,
		Pure,
		Spell,
		
		/// <summary>
		/// Except flag for better experience in the config
		/// </summary>
		ExFlag,
	}
}