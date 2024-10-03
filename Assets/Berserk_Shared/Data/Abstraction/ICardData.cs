using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Abstraction
{
	public interface ICardData : IObjectData
	{
		Season Season { get; set; }
		Rarity Rarity { get; set; }
		Race[] Races { get; set; }
		Faction[] Factions { get; set; }
		SubType[] SubTypes { get; set; }
		List<string> IresurrectableIds { get; }
		int LimitInDeck { get; set; }
		int AddAtRegistration { get; set; }
		string Artist { get; set; }
		string Description { get; set; }
		string Tooltip { get; set; }
	}
}