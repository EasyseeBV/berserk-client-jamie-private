using System;
using System.Collections.Generic;

namespace ServerCore.Infrastructure.Models
{
	[Obsolete("Client not used this model")]
	public class EntityStateModel
	{
		public List<DynamicEffectModel> CurrentEffects { get; set; } = new List<DynamicEffectModel>();
		public string Id { get; set; } // UID. The "Id" name is used for mapping. Warning - changing the name will break the mapping
		public string OwnerUserName { get; set; }
		public int RoundNumber { get; set; }
		public int CurrentHealth { get; set; }
		public int MaxHealth { get; set; }
		public int CurrentAttack { get; set; }
		public int MaxAttack { get; set; }
		public int CurrentMana { get; set; }
		public int MaxMana { get; set; }
		public bool IsCanAttack { get; set; }
		public bool IsCanUseAbility { get; set; }
	}
}