using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.Data.Game
{
	public class HeroData : IHeroData, IConfigData
	{
		// data
		public string Id { get; set; }
		
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Hp { get; set; }
		public int Mana { get; set; }
		public int LevelAtSync { get; set; }
		public int LevelAtRegistration { get; set; }
		public ObjectType Type { get; set; }
		public ArtType ArtType { get; set; }

		// info
		public string ArtUrl { get; set; }
		public string Title { get; set; }
		public string Name { get; set; }
		public Quadrant Quadrant { get; set; }
		public List<string> EffectsIds { get; set; } = new();
		public List<InvalidAction> InvalidActions { get; set; } = new();

		public override string ToString()
		{
			return this.ReflectionFormat();
		}
	}
}
