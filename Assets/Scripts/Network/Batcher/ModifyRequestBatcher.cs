using System.Collections.Generic;
using Game.Entities;
using ServerCore.Infrastructure.Models;
using Vulcan.Data;
using Vulcan.Network.Context;

namespace Vulcan.Network
{
	public sealed class ModifyRequestBatcher
	{
		public static Batch PrepareBatch(IInteractiveEntity mutableEntity)
		{
			var data = mutableEntity.ToEntityStateModel();

			var model = new ModifyEntityModel
			{
				SessionPlayerId = ActorsContextResolver.Self.Id,
				EntityStates = new List<EntityStateModel> { data }
			};

			return new Batch(model, MessageType.Modify, ActorsContextResolver.Self.UserName);
		}
	}
}