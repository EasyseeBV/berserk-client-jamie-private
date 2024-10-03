using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Game
{
	public class RuntimeData : IRuntimeData
	{
		public int Id { get; set; }
		public string DataId { get; set; }
		public ObjectType Type { get; set; }
		public string OwnerUserId { get; set; }
		public IntStat Hp { get; set; }
		public IntStat Mana { get; set; }
		public IntStat Attack { get; set; }
		public IntStat Armor { get; set; }
		public IntStat MoveCount { get; set; }

		/// <summary>
		///     keywords and effects that already applied over this card.
		/// </summary>
		public List<IRuntimeEffectData> AppliedEffects { get; set; } = new();

		public List<string> AppliedEffectsIds { get; set; } = new();

		/// <summary>
		///     effects that card can impose/apply over the target.
		/// </summary>
		public List<string> ImposingEffects { get; set; } = new();

		/// <summary>
		///     keywords and effects that card is immune to (wont apply to object).
		/// </summary>
		public List<ImmuneKeyword> ImmuneToKeywords { get; set; } = new();

		/// <summary>
		///     Damage types that wont affect this object
		/// </summary>
		public List<DamageValue> ImmuneToDamage { get; set; } = new();

		public List<string> GetInnateEffectsIds()
		{
			return AppliedEffects.Where(effect => effect.IsInnate).Select(x => x.ConfigId).Concat(ImposingEffects)
				.ToList();
		}

		public List<IRuntimeEffectData> GetGainedEffects()
		{
			var gainedEffects =
				AppliedEffects.Where(effect => !effect.IsInnate).ToList();

			return gainedEffects;
		}

		[JsonConstructor]
		public RuntimeData() {}

		public RuntimeData(IObjectData data)
		{
			DataId = data.Id;
			Hp = new IntStat(data.Hp);
			Mana = new IntStat(data.Mana);
			Attack = new IntStat(data.Attack);
			Armor = new IntStat(data.Armor); // empty stat
			MoveCount = new IntStat(0, 1); // max 1 move per turn
			ImposingEffects = data.EffectsIds?.ToList();
			Type = data.Type;

			Hp.SetName(nameof(Hp));
			Mana.SetName(nameof(Mana));
			Attack.SetName(nameof(Attack));
			Armor.SetName(nameof(Armor));
			MoveCount.SetName(nameof(MoveCount));
		}

		public bool Equals(IRuntimeData other)
		{
			return Id == other?.Id;
		}

		public virtual void Dispose()
		{
			Hp.Dispose();
			Mana.Dispose();
			Attack.Dispose();
			Armor.Dispose();
			MoveCount.Dispose();
			AppliedEffects.Clear();
			ImposingEffects.Clear();
			ImmuneToKeywords.Clear();
			ImmuneToDamage.Clear();
		}
	}
}