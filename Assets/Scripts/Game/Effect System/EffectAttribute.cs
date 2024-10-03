using System;
using Berserk.Shared.Data.Enums;
using Vulcan.Data;

namespace Game.Effect_System
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class EffectAttribute : Attribute
	{
		public EffectKeyword Effect { get; }
		public EffectFamily Family { get; }

		public EffectAttribute(EffectKeyword effect, EffectFamily family = EffectFamily.None)
		{
			Effect = effect;
			Family = family;
		}
	}
}