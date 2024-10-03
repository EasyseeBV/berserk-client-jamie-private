using System;
using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.EffectsVisual.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EffectVisualAttribute : Attribute
	{
		public EffectVisualKeyword Keyword { get; }

		public EffectVisualAttribute(EffectVisualKeyword keyword)
		{
			Keyword = keyword;
		}
	}
}