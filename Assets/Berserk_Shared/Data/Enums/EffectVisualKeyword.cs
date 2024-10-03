using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	/// <summary>
	/// Visual sequence keyword. Sequence of vfx and/or animations used to build complex visual effects.
	/// The order is matter: lower items in the list has more draw priority.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectVisualKeyword
	{
		None = 0,
		DamageAllCards_HA,
		DamageRandomCard_HA,
		Blacksmith,
		Buff,
		BuffHealth,
		BuffAttack,
		SpellDamage_HA,
		DamageOpponent_HA,
		DestroyCard_HA,
		Empower,
		ExtraHealth_HA,
		GenericAttack,
		Heal,
		Hex,
		Hex_HA,
		Undefeatable_HA,
		LifeSteal,
		Parry,
		Poisoning,
		Rage,
		Raving,
		Reborn,
		Reflect,
		Revenge,
		Stunning,
		Disarm_HA,
		EvilEye_HA,
		Silence,
		Silence_HA,
		SilenceHero_HA,
		Slash,
		Sleeping,
		Stealth,
		Summon,
		SwapStats,
		Taunting,
		Vendetta,
		Sanctification,
		ArrangeInShowAll,
		DiscardHand,
		SunfireStrike,
		Overcharge,
		Mulligan,
		Destruction,
		Exile,
		CardSpawn,
		TakeHeal,
		TakeHit,
		Mark,
		ResurrectX,
		DiscardHandRandom,
		Stasis,
		Immortal,
		ImmuneToPoisoning,
		ImmuneToSleeping,
	}
}