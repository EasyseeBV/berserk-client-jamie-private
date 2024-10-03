namespace Events
{
	/// <summary>
	///     Used by <see cref="GameBus.OnActionBlocked" />
	/// </summary>
	public enum BlockedInfo
	{
		None,
		NoSpellTarget,
		Immune,
		DeckExhausted,
		ImmuneCantTargetTaunt,
		ShouldTargetTaunt,
		AbilityHasBeenUsed,
		AbilityStunned,
		NotEnoughLava,
		NoCreatureTarget,
		OpponentDisconnect,
		OpponentConnect
	}
}