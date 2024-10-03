using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Game
{
	public class RuntimeHeroData : RuntimeData, IRuntimeHeroData
	{
		public IntStat AbilityMoveCount { get; set; }

		[JsonConstructor]
		public RuntimeHeroData()
		{
		}

		public RuntimeHeroData(IObjectData data) : base(data)
		{
			AbilityMoveCount = new IntStat(1, 1);
			AbilityMoveCount.SetName(nameof(AbilityMoveCount));
		}

		public override void Dispose()
		{
			base.Dispose();
			AbilityMoveCount.Dispose();
		}
	}
}