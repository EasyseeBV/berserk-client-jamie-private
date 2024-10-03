using Berserk.Shared.Data.Abstraction;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class AddImmuneToKeyword : LogicEvent
	{
		public string Keyword { get; }
		public int EffectId { get; }
		public int RuntimeId { get; }

		[JsonConstructor]
		public AddImmuneToKeyword(string keyword, int runtimeId, int effectId)
		{
			Keyword = keyword;
			RuntimeId = runtimeId;
			EffectId = effectId;
		}

		public AddImmuneToKeyword(ImmuneKeyword immuneKeyword, int runtimeId)
		{
			Keyword = immuneKeyword.Keyword;
			RuntimeId = runtimeId;
			EffectId = immuneKeyword.EffectId;
		}
	}
}