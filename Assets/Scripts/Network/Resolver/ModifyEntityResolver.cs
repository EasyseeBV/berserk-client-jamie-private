using System.Collections;
using Events;
using Game.Entities;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Network.Resolver
{
	public static class ModifyEntityResolver
	{
		public static void Resolve(EntityStateModel[] remoteStates, long timestamp)
		{
			remoteStates.ForEach(state => Resolve(state, timestamp));
		}

		private static void Resolve(EntityStateModel remoteState, long timestamp)
		{
			if (!ResolverHelpers.CanResolve(remoteState))
				return;

			if (!ResolverHelpers.TryFindEntityOnBoard(remoteState.Id, out var entity))
			{
				RRLogger.Log($"[{"Resolver".Orange().Bold()}] {nameof(IInteractiveEntity)} - {remoteState.Id} on board not found");
				return;
			}

			EventQueue.Enqueue(new GameEvent(GameEventType.ModifyEntity, ResolveEntityRoutine(), timestamp));

			IEnumerator ResolveEntityRoutine()
			{
				if (!ResolverHelpers.ValidateEntity(entity, remoteState))
					ResolverHelpers.UpdateEntity(entity, remoteState);
				yield return null;
			}
		}
	}
}