using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Fortitude)]
	[EffectKeyword(EffectKeyword.IgnoreTaunting)]
	[EffectKeyword(EffectKeyword.Immortal)]
	[EffectKeyword(EffectKeyword.Taunting)]
	[EffectKeyword(EffectKeyword.Sanctification)]
	[EffectKeyword(EffectKeyword.Petrify)]
	public class FlagEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			//not logic, only flag
		}
	}
}