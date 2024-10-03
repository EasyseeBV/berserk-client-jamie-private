using System;

namespace ServerCore.Infrastructure.Models
{
	[Obsolete("Client not used this model")]
	public class InteractiveCardModel
	{
		public string Id { get; set; }
		public string CardId { get; set; }

		public string SessionPlayerGuid { get; set; }
		public string OwnerUserName { get; set; }

		public bool IsSummonedByEffect { get; set; }
		public EntityStateModel EntityState { get; set; }
	}
}