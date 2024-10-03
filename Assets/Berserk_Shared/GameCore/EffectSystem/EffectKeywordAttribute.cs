using System;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EffectKeywordAttribute : Attribute
	{
		public EffectKeyword Keyword;

		public EffectKeywordAttribute(EffectKeyword keyword)
		{
			Keyword = keyword;
		}
	}
}