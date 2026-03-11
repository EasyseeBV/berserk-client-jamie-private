using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Game
{
	public class RuntimeHeroData : RuntimeData, IRuntimeHeroData
	{
		public IntStat AbilityMoveCount { get; set; }
		public int AbilityManaCost { get; set; } = 0;
		public int AbilityHpCost { get; set; } = 0;

		[JsonConstructor]
		public RuntimeHeroData()
		{
		}

		public RuntimeHeroData(IObjectData data) : base(data)
		{
			AbilityMoveCount = new IntStat(1, 1);
			AbilityMoveCount.SetName(nameof(AbilityMoveCount));
			
			if (data is not IHeroData heroData)
				return;

			AbilityManaCost = heroData.AbilityManaCost;
			AbilityHpCost = heroData.AbilityHpCost;
		}

		public override void Dispose()
		{
			base.Dispose();
			AbilityMoveCount.Dispose();
		}
	}
}