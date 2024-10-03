using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IRuntimeData : IEquatable<IRuntimeData>, IDisposable, IRuntimeDataBase
	{
		int Id { get; set; }
		string DataId { get; set; }
		ObjectType Type { get; set; }
		string OwnerUserId { get; set; }

		IntStat Hp { get; set; }
		IntStat Mana { get; set; }
		IntStat Attack { get; set; }
		IntStat Armor { get; set; }
		IntStat MoveCount { get; set; }

		List<string> ImposingEffects { get; set; }
		List<IRuntimeEffectData> AppliedEffects { get; set; }

		List<ImmuneKeyword> ImmuneToKeywords { get; set; }
		List<DamageValue> ImmuneToDamage { get; set; }

		List<string> GetInnateEffectsIds();
		List<IRuntimeEffectData> GetGainedEffects();
	}
}