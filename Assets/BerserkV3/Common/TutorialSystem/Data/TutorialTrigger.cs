namespace BerserkV3.Common.TutorialSystem
{
    public enum TutorialTrigger // gray keywords configured from scene or used implicitly
    {
	    #region General
	    Start,
	    StartSession,
	    #endregion

	    #region GameStart
	    Welcome,
	    CardInfo,
	    GeneralFlow,
	    #endregion

	    #region Mulligan
	    ReadyToMulligan,
	    MulliganRect,
	    MulliganReplace,
	    MulliganAccept,
	    MulliganFinish,
	    #endregion

	    #region GamePlay
	    GameBoard,
	    GameDeck,
	    GameGraveyard,
	    GameHandSelf,
	    GameLava,
	    GameTimer,
	    SleepingEffect,
	    HeroBounds,
	    HeroHealth,
	    HeroAttack,
	    HeroArmor,
	    CreatureHealth,
	    CreatureAttack,
	    CreatureArmor,
	    CreatureBounds,
	    FullCardBounds,
	    BotContinue,
	    EffectPlay,
	    EffectAdd,
	    HoverCards,
	    PreviewCard,
	    SessionEnd,
	    ReceiveEndGame,
	    EndGame,
	    #endregion

	    #region Lobby
	    FirstVulcanite,
		DecksBtn,
		DeckVulcaniteFlag,
		DeckFactionFlag,
		DeckLeagueFlags,
		DeckNewBtn,
		DeckEditBtn,
		DeckEditMenuExit,
		DeckLavaCurve,
		DeckCurrentCards,
		DeckAvailableCards,
		DeckVulcaniteChange,
		DuelsBtn,
		DuelsMenuExit,
		LeaguesBtn,
		LeagueMmr,
		LeagueFlags,
		LeagueDecks,
		TutorialEnd,
	    #endregion
    }
}