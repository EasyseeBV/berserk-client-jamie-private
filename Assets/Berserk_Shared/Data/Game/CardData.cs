using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.Data.Game
{
	[Serializable]
	public class CardData : ICardData, IConfigData
	{
		// data
		public string Id { get; set; }
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Hp { get; set; }
		public int Mana { get; set; }
		public List<string> EffectsIds { get; set; } = new();
		public List<string> IresurrectableIds { get; set; } = new();
		public List<InvalidAction> InvalidActions { get; set; } = new();
		public ObjectType Type { get; set; }
		public Season Season { get; set; }
		public Rarity Rarity { get; set; }
		public Race[] Races { get; set; }
		public Faction[] Factions { get; set; }
		public SubType[] SubTypes { get; set; }
		public ArtType ArtType { get; set; }
		public Quadrant Quadrant { get; set; }
		public int LimitInDeck { get; set; }
		public int AddAtRegistration { get; set; }
		
		// Info
		public string Title { get; set; }
		public string ArtUrl { get; set; }
		public string Artist { get; set; }
		public string Description { get; set; }
		public string Tooltip { get; set; }

		public override string ToString()
		{
			return this.ReflectionFormat();
		}
	}
}
