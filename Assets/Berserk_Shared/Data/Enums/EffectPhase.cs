using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectPhase
	{
		None,
		GameStart,
		FirstTurn,
		BeforeDead,
		BeforeEachRound,
		BeforeNextRound,
		RoundStarted,
		BeforeEndRound,
		BeforeAttack,
		BeforeDamaged,
		BeforeDefense,
		BeforeDiscard,
		Counterattack,
		AbilityButtonPress, // used only on client side
		PlayerManaChanged,
		AfterResurrection,
		AfterSilence,
		AfterSpawn,
		AfterAttack,
		AfterDamaged,
		AfterDefense,
		AfterDiscard,
		AfterKill,
		AfterDeath,
		AfterDraw,
		AfterHeal,
		AfterDisable,
		AfterHeroHealed,
		#region Logic state accept phases

		// Use it only with effects which created trough GiveEffect.
		// It excepts phase handle from other effects already in use.

		InShowAccepted,
		InChooseAccepted,
		InDiscardAccepted,

		#endregion

	}
}