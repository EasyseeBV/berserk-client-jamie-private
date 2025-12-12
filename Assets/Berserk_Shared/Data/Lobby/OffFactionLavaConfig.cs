using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby
{
	public class OffFactionLavaConfig
	{
		public bool Enabled { get; set; }
		public int PenaltyPerCard { get; set; }
		public Quadrant[] NeutralQuadrants { get; set; }
	}
}