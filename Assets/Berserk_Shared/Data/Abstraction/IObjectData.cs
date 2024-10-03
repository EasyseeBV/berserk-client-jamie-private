using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IObjectData
	{
		// data
		string Id { get; }
		int Hp { get; }
		int Armor { get; }
		int Mana { get; }
		int Attack { get; }
		ObjectType Type { get; }
		ArtType ArtType { get; set; }
		Quadrant Quadrant { get; set; }
		
		// info
		string Title { get; }
		string ArtUrl { get; }
		List<string> EffectsIds { get; }
		List<InvalidAction> InvalidActions { get; }
	}
}