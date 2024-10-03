using System;
using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Abstraction
{
	public class DamageValue
	{
		public DamageType DamageType { get; }
		public EffectValue ValueMod { get; }
		public int EffectId { get; }
		public int Value { get; }
		
		[JsonConstructor]
		public DamageValue(DamageType damageType, EffectValue valueMod, int effectId, int value)
		{
			DamageType = damageType;
			ValueMod = valueMod;
			EffectId = effectId;
			Value = value;
		}

		public virtual int GetDamage(int rawDamageValue)
		{
			return rawDamageValue;
		}

		public virtual bool IsFullResist()
		{
			return false;
		}
		
		public override bool Equals(object obj)
		{
			if (obj is DamageType damageType)
				return DamageType == damageType;
			
			return obj is DamageValue damageValue 
			       && damageValue.DamageType == DamageType
			       && damageValue.EffectId == EffectId;
		}
		
		public override int GetHashCode()
		{
			return HashCode.Combine((int) DamageType, EffectId);
		}
		
		public override string ToString()
		{
			return $"EffectId:{EffectId}, DamageType:{DamageType}";
		}
	}
}
