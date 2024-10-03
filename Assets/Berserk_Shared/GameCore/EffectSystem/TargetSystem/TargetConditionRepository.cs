using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.TargetSystem
{
	public interface ITargetConditionRepository
	{
		event Action<InvalidAction> OnInvalidAction;
		
		IRuntimeGameObject[] GetAllowedTargets(
			IRuntimeGameObject executor, 
			EffectData effect, 
			params IRuntimeGameObject[] manualTargets);
		
		IRuntimeGameObject[] GetAllowedTargets(
			IRuntimeGameObject executor, 
			string effectId, 
			params IRuntimeGameObject[] manualTargets);
		
		bool IsAllowedTarget(
			IRuntimeGameObject executor, 
			IRuntimeGameObject target, 
			string effectId, 
			params IRuntimeGameObject[] manualTargets);
		
		bool IsAllowedTarget(
			IRuntimeGameObject executor, 
			IRuntimeGameObject target, 
			EffectData effect, 
			params IRuntimeGameObject[] manualTargets);
		
		bool IsFullTable(string userId, int extraCount = 0);
		
		int GetFreeTableSpace(string userId, int extraCount = 0);
	}

	public class TargetConditionRepository : ITargetConditionRepository
	{
		public event Action<InvalidAction> OnInvalidAction;
		
		private readonly IGameContext gameContext;
		private IGameRuntimePool GameRuntimePool => gameContext.GameRuntimePool;
		private IGameDatabase GameDatabase => gameContext.GameDatabase;
		private ISharedConfig SharedConfig => gameContext.SharedConfig;

		public TargetConditionRepository(IGameContext gameContext)
		{
			this.gameContext = gameContext;
		}

		public IRuntimeGameObject[] GetAllowedTargets(
			IRuntimeGameObject executor, 
			EffectData effect, 
			params IRuntimeGameObject[] manualTargets)
		{
			return GameRuntimePool
				.Where(target => IsAllowedTargetInternal(null, executor, target, effect, manualTargets))
				.ApplyParamFilter(executor, effect.ParamFilter, effect.ParamFilterValue)
				.ToArray();
		}

		public IRuntimeGameObject[] GetAllowedTargets(
			IRuntimeGameObject executor, 
			string effectId, 
			params IRuntimeGameObject[] manualTargets)
		{
			return GetAllowedTargets(executor, GameDatabase.GetEffectConfig(effectId), manualTargets);
		}

		// Do not use in TargetConditionRepository, use instead IsAllowedTargetInternal
		public bool IsAllowedTarget(
			IRuntimeGameObject executor, 
			IRuntimeGameObject target, 
			string effectId, 
			params IRuntimeGameObject[] manualTargets)
		{
			return IsAllowedTarget(executor, target, GameDatabase.GetEffectConfig(effectId), manualTargets);
		}
		
		// Do not use in TargetConditionRepository, use instead IsAllowedTargetInternal
		public bool IsAllowedTarget(
			IRuntimeGameObject executor, 
			IRuntimeGameObject target, 
			EffectData effect, 
			params IRuntimeGameObject[] manualTargets)
		{
			var externChecks = new Func<EffectData, bool>(e => target.ApplyParamFilter(executor, e.ParamFilter, e.ParamFilterValue).Any());
			return IsAllowedTargetInternal(externChecks, executor, target, effect, manualTargets);
		}

		public bool IsFullTable(string userId, int extraCount = 0)
		{
			return GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, userId, ObjectType.TableCardsMask)
				.Count() + extraCount >= SharedConfig.MaxCardsOnTable;
		}

		public int GetFreeTableSpace(string userId, int extraCount = 0)
		{
			return Math.Max(0, SharedConfig.MaxCardsOnTable - GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, userId, ObjectType.TableCardsMask)
				.Count() + extraCount);
		}
		
		private bool IsAllowedTargetFilter(
			EffectData effectData,
			IRuntimeGameObject executor,
			IRuntimeGameObject target,
			params IRuntimeGameObject[] manualTargets)
		{
			if (effectData.TargetFilters is not {Length: >0})
				return true;
			
			return effectData.TargetFilters.All(targetFilter =>
			{
				return targetFilter switch
				{
					EffectTargetFilter.ExceptSelf => target != executor,
					EffectTargetFilter.ExceptTarget => manualTargets == null || manualTargets.Length == 0 || !manualTargets.Contains(target),
					EffectTargetFilter.ExceptTurnOwner => target.RuntimeData.OwnerUserId != gameContext.Timer.RuntimeData.OwnerId,
					EffectTargetFilter.OnlyTurnOwner => target.RuntimeData.OwnerUserId == gameContext.Timer.RuntimeData.OwnerId,
					EffectTargetFilter.None => true,
					_ => throw new NotImplementedException($"Unknown {nameof(EffectTargetFilter)} : {targetFilter}")
				};
			});
		}

		/// <summary>
		/// Internal checks, excluded ParamFilter - this check as post process
		/// </summary>
		/// <param name="extraCheck">external check function</param>
		/// <param name="executor">effect owner</param>
		/// <param name="target">target to check</param>
		/// <param name="effectData">data to check allowed targets</param>
		/// <param name="manualTargets">other manual targets</param>
		/// <returns></returns>
		private bool IsAllowedTargetInternal(
			Func<EffectData, bool> extraCheck, 
			IRuntimeGameObject executor,
			IRuntimeGameObject target, 
			EffectData effectData,
			params IRuntimeGameObject[] manualTargets)
		{
			if (effectData == null)
			{
				DefaultSharedLogger.Error($"[IsAllowedTarget] Effect data does not exist, executor : {executor?.Data.Title}, target : {target?.Data.Title}");
				return false;
			}

			if (!IsAllowedTroughDeath(target, effectData))
				return false;

			if (!IsAllowedTargetLimit(target, effectData))
				return false;

			if (!IsAllowedTargetType(target, effectData))
				return false;

			if (!IsAllowedTargetFaction(target, effectData))
				return false;

			if (!IsAllowedTargetSubType(target, effectData))
				return false;

			if (!IsAllowedTargetRace(target, effectData))
				return false;

			if (!IsAllowedTargetQuadrant(target, effectData))
				return false;

			//This check should not be moved! Because uses data from GameRuntimePool without effect filters 
			if (!CanAffectOnTarget(target, effectData))
				return false;
			
			if (!IsAllowedTargetOwner(executor, target, effectData))
				return false;

			if (!IsAllowedGenericAttack(executor, target, effectData))
				return false;

			if (!CheckStealth(executor, target, effectData))
				return false;

			if (!CheckTaunt(executor, target, effectData))
				return false;

			if (!IsAllowedTargetFilter(effectData, executor, target, manualTargets))
				return false;

			if (!(extraCheck == null || extraCheck.Invoke(effectData)))
				return false;

			return true;
		}
		
		private bool IsAllowedTargetOwner(IRuntimeGameObject executor, IRuntimeGameObject target, EffectData effectData)
		{
			return effectData.TargetOwner == Owner.None
			       || (effectData.TargetOwner == Owner.Self && executor.RuntimeData.OwnerUserId == target.RuntimeData.OwnerUserId)
			       || (effectData.TargetOwner == Owner.Opponent && executor.RuntimeData.OwnerUserId != target.RuntimeData.OwnerUserId);
		}

		private bool IsAllowedTargetLimit(IRuntimeGameObject target, EffectData effectData)
		{
			if (effectData.TargetLimits is not {Length: > 0})
				return true;

			if (target is IRuntimeGameCard gameCard)
				return effectData.TargetLimits.Contains(gameCard.RuntimeData.State);

			if (target.Data.Type == ObjectType.Hero)
				return effectData.TargetLimits.Contains(RuntimeState.InTable);
			
			throw new NotImplementedException($"Unknown {nameof(ObjectType)} : {target.Data.Type}");
		}

		private bool IsAllowedTargetType(IRuntimeGameObject target, EffectData effectData)
		{
			return effectData.TargetTypes.AnyExcept(ObjectType.ExFlag, ObjectType.None, target.Data.Type);
		}

		private bool IsAllowedTargetFaction(IRuntimeGameObject target, EffectData effectData)
		{
			return target is not IRuntimeGameCard gameCard 
			       || effectData.TargetFactions.AnyExcept(Faction.ExFlag, Faction.None, gameCard.Data.Factions);
		}

		private bool IsAllowedTargetSubType(IRuntimeGameObject target, EffectData effectData)
		{
			return target is not IRuntimeGameCard gameCard 
			       || effectData.TargetSubTypes.AnyExcept(SubType.ExFlag, SubType.None, gameCard.Data.SubTypes);
		}

		private bool IsAllowedTargetRace(IRuntimeGameObject target, EffectData effectData)
		{
			return target is not IRuntimeGameCard gameCard
			       || effectData.TargetRaces.AnyExcept(Race.ExFlag, Race.None, gameCard.Data.Races);
		}

		private bool IsAllowedTargetQuadrant(IRuntimeGameObject target, EffectData effectData)
		{
			return effectData.TargetQuadrants.AnyExcept(Quadrant.ExFlag, Quadrant.None, target.Data.Quadrant);
		}

		private bool IsAllowedTroughDeath(IRuntimeGameObject target, EffectData effectData)
		{
			return !target.IsDead 
			       || effectData.Keyword is EffectKeyword.LifeSteal or EffectKeyword.Barricade 
			       || effectData.TargetLimits.Any(runtimeState => runtimeState is RuntimeState.InDiscard or RuntimeState.InExile);
		}

		private bool IsAllowedGenericAttack(IRuntimeGameObject executor, IRuntimeGameObject target, EffectData effectData)
		{
			return effectData.Keyword != EffectKeyword.GenericAttack || executor.RuntimeData.MoveCount > 0;
		}

		private bool CheckStealth(IRuntimeGameObject executor, IRuntimeGameObject target, EffectData effectData)
		{
			var noStealth = executor.RuntimeData.OwnerUserId == target.RuntimeData.OwnerUserId
			                || !target.HasAppliedNonDisabledEffect(EffectKeyword.Stealthing)
			                || effectData.Aoe != EffectAoe.Single // aoe will attack anyway
			                || effectData.TargetMod == EffectTargetMod.Random;

			if (!noStealth)
				OnInvalidAction?.Invoke(InvalidAction.CantTargetStealth);

			return noStealth;
		}

		private bool CheckTaunt(IRuntimeGameObject executor, IRuntimeGameObject target, EffectData effectData)
		{
			if (effectData.Keyword != EffectKeyword.GenericAttack)
				return true;

			if (executor.HasAppliedNonDisabledEffect(EffectKeyword.IgnoreTaunting))
				return true;

			var taunts = GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, target.RuntimeData.OwnerUserId, asQuery: true)
				.Where(card => card.HasAppliedNonDisabledEffect(EffectKeyword.Taunting))
				.ToArray();

			var noTaunts = !taunts.Any() || taunts.Contains(target);

			if (!noTaunts)
				OnInvalidAction?.Invoke(InvalidAction.ShouldTargetTaunt);

			return noTaunts;
		}
		
		private bool CanAffectOnTarget(IRuntimeGameObject target, EffectData effectData)
		{
			var keywords = new[] {effectData.Keyword.ToString(), effectData.Id};
			var result = target.CanAffect(effectData.AffectType, effectData.DamageType, out var reason, keywords);
			if (reason != InvalidAction.None)
				OnInvalidAction?.Invoke(reason);
			
			return result;
		}
	}
}
