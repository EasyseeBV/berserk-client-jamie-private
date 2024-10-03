using System.Collections.Generic;

namespace ServerCore.Infrastructure.Models
{
	public class ModifyEntityModel
	{
		public string SessionPlayerId { get; set; }

		public List<EntityStateModel> EntityStates { get; set; } = new List<EntityStateModel>();
	}
}