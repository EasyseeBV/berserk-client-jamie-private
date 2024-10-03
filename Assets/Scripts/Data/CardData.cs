using Berserk.Shared.Data.Enums;

namespace Vulcan.Data
{
	public class CardData : DataBase
	{
		public Season Season;
		public string Description;
		public string TooltipCardText;
		public Race[] Races;
		public Faction[] Factions;
		public SubType[] SubTypes;
		public ArtType ArtType;
		public Rarity Rarity;
		public Quadrant Quadrant;
		public CardType Type;
		public string Artist;

		public string BackArtUrl;

		public int LimitInDeck;

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((CardData)obj);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public static bool operator ==(CardData obj1, CardData obj2)
		{
			return obj1?.Equals(obj2) ?? ReferenceEquals(obj2, null);
		}

		public static bool operator !=(CardData obj1, CardData obj2)
		{
			return !(obj1 == obj2);
		}
	}
}