using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum SubType // DO NOT REMOVE UNDERSCORE
	{
		None = 0,
		Rage,
		Double_Strike,
		Lifesteal,
		Stun,
		AOE_Attack,
		Taunt,
		Buff,
		Summon,
		Reborn,
		Attack_Spell,
		Heal,
		Lava_Gain,
		Undead,
		Card_Draw,
		Player_Heal,
		Random_Attack,
		Poison,
		Debuff,
		Stoneskin,
		Token,
		Vendetta,
		Silence,
		Player_Lifesteal,
		Parry,
		Attack_spell,
		Revenge,
		Random_Card_Graveyard,
		Stealth,
		Player_Armour_Buff,
		Confused,
		Lava_gain,
		AOE_Attack_Spell,
		
		/// <summary>
		/// Except flag for better experience in the config
		/// </summary>
		ExFlag = -1,
	}
}