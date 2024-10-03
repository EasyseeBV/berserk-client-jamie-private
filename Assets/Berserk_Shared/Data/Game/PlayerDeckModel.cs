using System.Collections.Generic;

namespace Berserk.Shared.Data.Game
{
	public class PlayerDeckModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public List<string> CardIds { get; set; } = new();
		public string HeroId { get; set; }
	}
}