using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.EffectSystem.Effects;

namespace BerserkV3.GameCore.SharedImplementations
{
	public class ClientEffectsFactory : EffectsFactory
	{
		public override KeywordEffect Create(EffectKeyword keyword)
		{
			keyword = EffectKeyword.None; // always the client uses Mock. delete when integrating predicts
			
			if (!EffectTypes.TryGet(keyword, out var effectType))
				throw new ArgumentNullException($"{nameof(keyword)} {keyword} not found");

			if (effectType == null)
				throw new ArgumentNullException(
					$"There is no {nameof(keyword)} class type matching the effect {keyword}!");

			return (KeywordEffect)Activator.CreateInstance(effectType);
		}
	}
}