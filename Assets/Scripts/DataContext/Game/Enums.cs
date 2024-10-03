using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vulcan.Data
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CardType
	{
		Creep = 0,
		Spell = 1
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectAoe
	{
		Single = 0,
		AoeAll = 1,
		AoeAllExceptSelf = 2,
		AoeFromTarget = 3,
		AoeAllExceptTarget = 4
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ActionType
	{
		None = 0,
		Attack = 1,
		Effect = 2,

		//Server Usage
		Summon = 3
	}

	[Flags, JsonConverter(typeof(StringEnumConverter))]
	public enum EffectPhase
	{
		None = 0,
		BeforeDead = 1 << 0,
		AfterDead = 1 << 1,
		BeforeNextRound = 1 << 2,
		BeforeAttack = 1 << 5,
		AfterAttack = 1 << 6,
		OnAfterSpawnOnTable = 1 << 8,
		OnAbilityButtonPress = 1 << 12,
		OnGameStart = 1 << 13, //Happens 1 time at game starts.
		OnBeforeDefence = 1 << 14, //When can be attacked.
		OnAfterDefence = 1 << 15, //When can be attacked.
		OnFirstTurn = 1 << 16, //When Self and Opponent have their first turns. Happens 1 times for each player.
		OnBeforeDamaged = 1 << 17, //When can take damage. But it doesn't matter if damage wont be approved.
		OnAfterDamaged = 1 << 18, //When damage approved.
		OnBeforeEndRound = 1 << 19
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectEndPhase
	{
		StartRound = 0,
		EndRound = 1,
		Use = 2
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectValue
	{
		Default = 0,
		Percent = 1
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectTargetMod
	{
		None = 0,
		PlayerPicked = 1,
		Random = 2,
		Self = 3,
		RandomExceptSelf = 4,
		EntityAttackPicked = 5
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectTargetLimit
	{
		None = 0,
		Hero = 1,
		Cards = 2,
		Graveyard = 3,
		GraveyardSelf = 4,
		GraveyardOpponent = 5
	}

	public enum EffectFamily
	{
		None,
		Summon,
		AbsorbDamage,
		IgnoreDamage
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum VisualEffect
	{
		Spawn = -1,
		Death = -2,
		None = 0,
	}
}