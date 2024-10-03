using Newtonsoft.Json;
using System.Linq;
using System;
using Berserk.Shared.Data.Enums;

namespace Vulcan.Data
{
	public class EffectDescription
	{
		public VisualEffect Effect;
		public string Description;
	}

	[Serializable]
	public class EffectData
	{
		public string Id; // intentionally left field

		public EffectKeyword Effect;
		public EffectVisualKeyword VisualEffect;
		public EffectTargetMod TargetMod;
		public EffectTargetLimit TargetLimit;
		public EffectPhase Phase;
		public EffectEndPhase EndPhase;
		public EffectValue ValueMod;

		public EffectAoe Aoe;
		public Owner TargetOwner;

		public int Value;
		public int Length;
		public string CardId;

		public EffectDescription EffectDescription;

		public bool IsManualPick => TargetMod == EffectTargetMod.PlayerPicked;

		public override string ToString()
		{
			var fields = GetType()
				.GetFields()
				.Where(x => x.Name != nameof(Effect))
				.Select(x => $"{x.Name}={x.GetValue(this)}");

			return $"{Effect}:{string.Join("|", fields)}";
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Id != null ? Id.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Effect.GetHashCode());
				hashCode = (hashCode * 397) ^ (TargetMod.GetHashCode());
				hashCode = (hashCode * 397) ^ (TargetLimit.GetHashCode());
				hashCode = (hashCode * 397) ^ (Phase.GetHashCode());
				hashCode = (hashCode * 397) ^ (EndPhase.GetHashCode());
				hashCode = (hashCode * 397) ^ (ValueMod.GetHashCode());
				hashCode = (hashCode * 397) ^ (Aoe.GetHashCode());
				hashCode = (hashCode * 397) ^ (TargetOwner.GetHashCode());
				hashCode = (hashCode * 397) ^ Value;
				hashCode = (hashCode * 397) ^ Length;
				hashCode = (hashCode * 397) ^ (CardId != null ? CardId.GetHashCode() : 0);
				return hashCode;
			}
		}

		public bool Equals(EffectData other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return string.Equals(Id, other.Id) && string.Equals(Effect, other.Effect) && string.Equals(TargetMod, other.TargetMod) && string.Equals(TargetLimit, other.TargetLimit) && string.Equals(Phase, other.Phase) && string.Equals(ValueMod, other.ValueMod) && string.Equals(Aoe, other.Aoe) && string.Equals(TargetOwner, other.TargetOwner) && Value == other.Value && Length == other.Length && string.Equals(CardId, other.CardId);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((EffectData)obj);
		}
	}
}