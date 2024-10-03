using Berserk.Shared.Data.Identity.Social;
using Berserk.Shared.Data.Lobby;

namespace Berserk.Shared.Data.Abstraction
{
	public interface ISharedConfig
	{
		public string Environment { get; set; }
		string Version { get; }
		public string VersionConfig { get; set; }
		int PlayerStartingMana { get; }
		int PlayerMaxMana { get; }
		int PlayerManaRoundIncrease { get; }
		int PlayerLimit { get; }
		int MaxCardsInDeck { get; }
		int MinCardsInDeck { get; }
		int MaxDeckCount { get; }
		int HandCardsStartCount { get; }
		int HandCardsLimit { get; }
		int MaxCardsOnTable { get; }
		int InGameTimerSec { get; }
		int InMilliganTimerSec { get; }
		int InGameReadyTimerSec { get; }
		int AutoMatchAcceptTimeoutMs { get; }
		int ClientsOffsetTimeSec { get; }
		int AfkTurnCountToLose { get; }
		int GameConnectionTimeout { get; }
		int GameLoadingPreviewTime { get; }
		DeckRarityValueModel[] DeckRarityValueModels { get; }
		DeckNftBonusModel[] DeckBonusValueModels { get; }
		ExternalProvider[] AvailableSocials { get; }

		bool Initialized { get; }
		void FillFromJson(string json);
		void FillFromInstance(object instance);
	}
}