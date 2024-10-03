using System;
using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IVisualEffectTypeCollection
	{
		bool TryGetType(EffectVisualKeyword value, out Type result);
	}
}