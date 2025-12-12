using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Identity.Social;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Utils;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicContext
{

	[Serializable]
	public class SharedConfig : ISharedConfig
	{
		public string Environment { get; set; }
		public string Version { get; set; }
		public string VersionConfig { get; set; }
		public int PlayerStartingMana { get; set; }
		public int PlayerMaxMana { get; set; }
		public int PlayerManaRoundIncrease { get; set; }
		public int PlayerLimit { get; set; }
		public int MaxCardsInDeck { get; set; }
		public int MinCardsInDeck { get; set; }
		public int MaxDeckCount { get; set; }
		public int HandCardsStartCount { get; set; }
		public int HandCardsLimit { get; set; }
		public int MaxCardsOnTable { get; set; }
		public int InGameTimerSec { get; set; }
		public int InMilliganTimerSec { get; set; }
		public int InGameReadyTimerSec { get; set; }

		public int AutoMatchAcceptTimeoutMs { get; set; }
		public int ClientsOffsetTimeSec { get; set; }
		public int AfkTurnCountToLose { get; set; }
		public int GameConnectionTimeout { get; set; }
		public int GameLoadingPreviewTime { get; set; }
		public DeckRarityValueModel[] DeckRarityValueModels { get; set; }
		public DeckNftBonusModel[] DeckBonusValueModels { get; set; }
		public ExternalProvider[] AvailableSocials { get; set; }
		public OffFactionLavaConfig OffFactionLavaConfig { get; set; }
		[JsonIgnore] public bool Initialized { get; private set; }

		public void FillFromJson(string json)
		{
			var jsonSettings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto,
				NullValueHandling = NullValueHandling.Ignore
			};
			var instance = JsonConvert.DeserializeObject(json, GetType(), jsonSettings);

			FillFromInstance(instance);
		}

		public void FillFromInstance(object instance)
		{
			this.Map(instance);
			Initialized = true;
		}

		public override string ToString()
		{
			return this.ReflectionFormat();
		}
	}
}