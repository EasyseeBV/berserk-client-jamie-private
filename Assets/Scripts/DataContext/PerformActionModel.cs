using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Vulcan.Data;
using EffectPhase = Vulcan.Data.EffectPhase;

namespace ServerCore.Infrastructure.Models
{
	public class PerformActionModel
	{
		public string SessionPlayerId { get; set; }
		public List<InteractiveCardModel> Targets { get; set; } = new List<InteractiveCardModel>();
		public InteractiveCardModel Source { get; set; }
		public ActionType ActionType { get; set; }
		public EffectPhase Phase { get; set; }
		
		public EffectKeyword Effect { get; set; }
		
		public bool DefenceDamage { get; set; }
	}
}