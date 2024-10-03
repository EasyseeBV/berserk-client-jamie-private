using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.Data.Game
{
	public class EffectData : IConfigData
	{
		public string Id { get; set; }
		public bool Applied { get; set; }
		public bool FirstTickApply { get; set; }
		public bool FirstTickExecute { get; set; }
		public EffectKeyword Keyword { get; set; }
		public EffectVisualKeyword VisualKeyword { get; set; }
		public EffectPhase[] Phases { get; set; }
		public EffectPhaseTrigger[] PhaseTriggers { get; set; }
		public RuntimeState[] PhaseLimits { get; set; }
		public DamageType[] PhaseDamageLimits { get; set; }
		public ExpirePhase[] ExpirePhases { get; set; }
		public EffectExecuteTarget[] ExecuteTargets { get; set; }
		public EffectTargetMod TargetMod { get; set; }
		public EffectTargetFilter[] TargetFilters { get; set; }
		public int MinTargetCount { get; set; }
		public int MaxTargetCount { get; set; }
		public Quadrant[] TargetQuadrants { get; set; }
		public Race[] TargetRaces { get; set; }
		public Faction[] TargetFactions { get; set; }
		public SubType[] TargetSubTypes { get; set; }
		public ObjectType[] TargetTypes { get; set; }
		public RuntimeState[] TargetLimits { get; set; }
		public Owner TargetOwner { get; set; }
		public EffectAoe Aoe { get; set; }
		public DamageType DamageType { get; set; }
		public EffectValue ValueMod { get; set; }
		public bool BatchedVisuals { get; set; }
		public int Value { get; set; }
		public EffectStack[] EffectStacks { get; set; }
		public int Length { get; set; }
		public ParamFilter ParamFilter { get; set; }
		public int ParamFilterValue { get; set; }
		public string Meta { get; set; }
		public string KeywordId { get; set; }
		public string IconUrl { get; set; }
		public EffectAffectionType AffectType { get; set; }

		public override string ToString()
		{
			return this.ReflectionFormat();
		}
	}
}