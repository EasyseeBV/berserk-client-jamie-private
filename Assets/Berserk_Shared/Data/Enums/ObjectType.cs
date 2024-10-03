using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter)), Flags]
	public enum ObjectType
	{
		None = 2,
		Building = 4,
		Creature = 8,
		Spell = 16,
		Hero = 32,
		
		// Merge pattern for the same entities type
		CardsMask = Creature | Building | Spell,
		TableCardsMask = Creature | Building,
		TableEntitiesMask = Creature | Hero | Building,
		
		/// <summary>
		/// Except flag for better experience in the config
		/// </summary>
		ExFlag = -1, 
	}
}