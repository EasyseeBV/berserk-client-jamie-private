using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using Game.Effect_System;
using Game.Entities;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using UnityEngine;
using Vulcan.Data;

namespace Vulcan.Network.Resolver
{
	public static class PerformActionResolver
	{
		private static readonly WaitForFixedUpdate wait = new WaitForFixedUpdate();

		public static void Resolve(PerformActionModel remote, long timestamp)
		{
			EventQueue.Enqueue(new GameEvent(GameEventType.PerformAction, ResolveActionRoutine(), timestamp));

			IEnumerator ResolveActionRoutine()
			{
				var findSourceResult = ResolverHelpers.TryFindEntityOnBoard(remote.Source.Id, out var localSource);
				if (!findSourceResult)
					localSource = ResolverHelpers.CreateEntityMock(remote.Source.Id, remote.Source);

				var localTargets = remote.Targets.Select(remoteTarget =>
				{
					if (!ResolverHelpers.TryFindEntityOnBoard(remoteTarget.Id, out var entity))
						entity = ResolverHelpers.CreateEntityMock(remoteTarget.Id, remoteTarget);

					return entity;
				}).ToList();

				yield return ResolveAction(localSource, localTargets, remote);
				yield return wait;
				
				if (findSourceResult && ResolverHelpers.CanResolve(remote.Source.EntityState))
					CompareEntities(localSource, remote.Source.EntityState);
				
				if (!remote.Targets.All(target => ResolverHelpers.CanResolve(target.EntityState)))
					yield break;

				for (var i = 0; i < localTargets.Count; i++)
					CompareEntities(localTargets[i], remote.Targets[i].EntityState);
			}

			void CompareEntities(IInteractiveEntity localEntity, EntityStateModel remoteState)
			{
				if (!ResolverHelpers.ValidateEntity(localEntity, remoteState))
					ResolverHelpers.UpdateEntity(localEntity, remoteState);
			}
		}

		private static IEnumerator ResolveAction(IInteractiveEntity source, List<IInteractiveEntity> targets,
			PerformActionModel remote)
		{
			switch (remote.ActionType)
			{
				case ActionType.None:
					source.DataBase.Hp.Set(remote.Source.EntityState.CurrentHealth);
					yield break;
				case ActionType.Attack:
					if (targets.Count != 1)
						RRLogger.Error($"[{"Resolver".Orange().Bold()}] Single attack targets more than 1");
					yield return source.YieldAttack(targets[0], remote.DefenceDamage);
					break;
				case ActionType.Summon:
				case ActionType.Effect:
					EffectHandler.Resolve(source, remote.Phase, remote.Effect, targets.ToArray());
					yield break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}