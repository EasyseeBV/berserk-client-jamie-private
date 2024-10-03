using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Race  // DO NOT REMOVE UNDERSCORE
	{
		None = 0,
		Fae,
		Beast,
		Elemental,
		Spell,
		Centaur,
		Human,
		Artefact,
		Harpy,
		Cyclops,
		Daemon,
		Lost_Shade,
		Strix,
		Spartoi,
		Gorgon,
		Shade,
		Dragon,
		Lamia,
		Hero,
		Kobaloi,
		Druid,
		Automaton,
		Building,
		
		/// <summary>
		/// Except flag for better experience in the config
		/// </summary>
		ExFlag = -1,
	}
}