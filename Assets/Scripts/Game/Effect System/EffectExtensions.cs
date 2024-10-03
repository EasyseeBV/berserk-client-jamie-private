using System;
using System.Collections.Generic;
using System.Reflection;
using Berserk.Shared.Data.Enums;
using Events;
using Game.Effect_System.Effects;
using Game.Effect_System.Target;
using Game.Entities;
using RR.Core.DebugSystem;
using Vulcan.Data;
using EffectTargetMod = Vulcan.Data.EffectTargetMod;
using Owner = Berserk.Shared.Data.Enums.Owner;

namespace Game.Effect_System
{
	public static class EffectExtensions
	{
		public static IEffectBehaviour ToBehaviour(this PickInfo pickInfo)
		{
			var behaviour = EffectHandler.Effects[pickInfo.EffectState.EffectData.Effect];
			behaviour.Fill(pickInfo);
			return behaviour;
		}

		//The spell has no View
		public static IInteractiveEntity GetRealOwner(this IInteractiveEntity owner)
		{
			return owner.View != null
				? owner
				: GameBus.LocalContext.GetVulcaniteByOwner(owner.DataBase.Owner);
		}

		public static Owner GetAdaptedOwner(this Owner effectOwner, Owner sourceOwner)
		{
			var adaptedOwner = sourceOwner switch
			{
				Owner.Self => effectOwner == Owner.Self ? Owner.Self : Owner.Opponent,
				Owner.Opponent => effectOwner == Owner.Self ? Owner.Opponent : Owner.Self,
				_ => throw new NullReferenceException("Owner is None")
			};

			return adaptedOwner;
		}

		public static Owner GetOppositeOwner(this Owner owner)
		{
			if (owner == Owner.None)
				return owner;
			return owner == Owner.Self ? Owner.Opponent : Owner.Self;
		}

		public static EffectFamily GetFamily(this EffectKeyword effect)
		{
			var type = EffectHandler.Effects[effect].GetType();
			var effectAttribute = (EffectAttribute)type.GetCustomAttribute(typeof(EffectAttribute), true);
			return effectAttribute.Family;
		}

		public static IEnumerable<IInteractiveEntity> GetTargets(this PickInfo pickInfo)
		{
			var data = pickInfo.EffectState.EffectData;
			var targetsHpDictionary = new Dictionary<IInteractiveEntity, int>(); //Entity, entity's HP
			var tryCount = 0;
			var targetCount = pickInfo.EffectState.Value;
			
			for (var i = 0; i < targetCount; i++)
			{
				if (tryCount > 25)
					//When all targets were killed
					break;

				IInteractiveEntity target;
				switch (data.TargetMod)
				{
					case EffectTargetMod.RandomExceptSelf:
						target = TargetResolver.ConditionHandler.GetRandomAllowedTargetExceptSelf(pickInfo.From, data, true);
						break;

					case EffectTargetMod.Random:
						target = TargetResolver.ConditionHandler.GetRandomAllowedTarget(pickInfo.From, data, true);
						break;
					
					case EffectTargetMod.EntityAttackPicked:
						if(pickInfo.Targets == null)
							continue;
						
						if(i >= pickInfo.Targets.Length)
							continue;
						
						target = pickInfo.Targets[i];
						break;

					default:
						RRLogger.Error($"EffectExtensions [GetTargets Switch] UnHandled TargetMod : {data.TargetMod}, Effect {data.Effect}");
						continue;
				}

				//Check same target was already killed
				if (targetsHpDictionary.ContainsKey(target))
				{
					if (targetsHpDictionary[target] <= 0)
					{
						i--;
						tryCount++;
						continue;
					}

					targetsHpDictionary[target] -= pickInfo.From.DataBase.Attack;
				}
				else
					targetsHpDictionary.Add(target, target.DataBase.Hp - pickInfo.From.DataBase.Attack);

				yield return target;
			}
		}
	}
}
