using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	public class EffectDataMock : EffectData
	{
		public EffectDataMock(EffectVisualKeyword visualKeyword)
		{
			VisualKeyword = visualKeyword;
			Phases = Array.Empty<EffectPhase>();
			PhaseTriggers = Array.Empty<EffectPhaseTrigger>();
			PhaseLimits = Array.Empty<RuntimeState>();
			PhaseDamageLimits = Array.Empty<DamageType>();
			ExpirePhases = Array.Empty<ExpirePhase>();
			ExecuteTargets = Array.Empty<EffectExecuteTarget>();
			TargetFilters = Array.Empty<EffectTargetFilter>();
			TargetQuadrants = Array.Empty<Quadrant>();
			TargetRaces = Array.Empty<Race>();
			TargetFactions = Array.Empty<Faction>();
			TargetSubTypes = Array.Empty<SubType>();
			TargetTypes = Array.Empty<ObjectType>();
			TargetLimits = Array.Empty<RuntimeState>();
		}
	}
}