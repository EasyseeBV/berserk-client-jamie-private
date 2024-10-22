using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Shop.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ConsumableType
	{
		GoldenTicket,
		GoldenWillTicket,
		FortuneGoldCoin,
		DeckExpansionTicket,
		XpBooster,
		MasteryBooster,
		SoftCurrencyBooster,
		BreedingBooster,
		NestingBooster,
		GeneticBooster,
		QuestBooster,
		BattlePassBooster,
		WinStreakBooster,
		FortuneBronzeCoin,
		FortuneSilverCoin
	}
}