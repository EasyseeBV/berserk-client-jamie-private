using Berserk.Shared.GameCore;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IRuntimeHeroData : IRuntimeData
	{
		IntStat AbilityMoveCount { get; set; }
		public int AbilityManaCost { get; set; }
		public int AbilityHpCost { get; set; }
	}
}