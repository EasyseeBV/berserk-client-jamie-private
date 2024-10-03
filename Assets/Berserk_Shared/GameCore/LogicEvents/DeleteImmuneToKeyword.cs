using Berserk.Shared.Data.Abstraction;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class DeleteImmuneToKeyword : LogicEvent
	{
		public string Keyword { get; }
		public int EffectId { get; }
		public int RuntimeId { get; }

		[JsonConstructor]
		public DeleteImmuneToKeyword(string keyword, int runtimeId, int effectId)
		{
			Keyword = keyword;
			RuntimeId = runtimeId;
			EffectId = effectId;
		}
		
		public DeleteImmuneToKeyword(ImmuneKeyword immuneKeyword, int runtimeId)
		{
			Keyword = immuneKeyword.Keyword;
			RuntimeId = runtimeId;
			EffectId = immuneKeyword.EffectId;
		}
	}
}