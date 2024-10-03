using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{

	[EffectKeyword(EffectKeyword.None)]
	public class KeywordEffectMock : KeywordEffect
	{
		protected override void OnExecute() {}
		protected override void TryMarkEffectBatching(int batchId, bool isStartBatch = true) {}
	}

}