using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem
{
	public interface IEffectExecutor
	{
		IEnumerable<IRuntimeEffect> CreateStartedEffects(IRuntimeGameObject executor);
		
		IRuntimeEffect CreateAppliedEffectAuto(
			string effectConfigId,
			IRuntimeGameObject effectOwner,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] manualTargets);
		
		IRuntimeEffect CreateAndExecuteEffect(
			string effectConfigId, 
			IRuntimeGameObject effectOwner, 
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets);

		public IRuntimeEffect CreateAndExecuteEffectAuto(
			string effectConfigId,
			IRuntimeGameObject effectOwner,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets);

		bool ExecuteEffect(IRuntimeEffect effectRuntime);
		bool ExecuteAppliedEffect(IRuntimeEffect appliedEffect);
		void ExecutePhase(PhaseInfo info);
		void ExpireAppliedEffects(IRuntimeGameObject executor, ExpirePhase expirePhase);
	}

	public class EffectExecutor : IEffectExecutor
	{
		private readonly IGameContext gameContext;
		private readonly IGameLogicContext gameLogicContext;

		public EffectExecutor(IGameContext gameContext, IGameLogicContext logicContext)
		{
			this.gameContext = gameContext;
			gameLogicContext = logicContext;
		}
		
		public IEnumerable<IRuntimeEffect> CreateStartedEffects(IRuntimeGameObject executor)
		{
			var effectList = new List<IRuntimeEffect>(gameContext.GameDatabase
				.GetEffects(executor.RuntimeData.ImposingEffects)
				.Where(data => (data.Applied && data.FirstTickApply) || (!data.Applied && data.FirstTickExecute))
				.Select(data => 
					!data.Applied ? 
					CreateAndExecuteEffectAuto(data.Id, executor) : 
					InternalCreateAndExecuteAppliedEffectFromImposingEffectAuto(data.Id, executor)));
			
			if (executor.Data.Type == ObjectType.Creature)
				effectList.Add(CreateAppliedEffectAuto(EffectKeyword.Sleeping.AsSystemEffectId(), executor));
			
			return effectList;
		}
		
		public IRuntimeEffect CreateAppliedEffectAuto(
			string effectConfigId, 
			IRuntimeGameObject effectOwner,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] manualTargets)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(effectConfigId);
			var autoAssignTargets = gameLogicContext.TargetResolver.GetAutoTargets(effectConfig, effectOwner, manualTargets);
			return InternalCreateAppliedEffectFor(effectConfigId, effectOwner, false, args, autoAssignTargets);
		}

		public IRuntimeEffect CreateAndExecuteEffect(
			string effectConfigId, 
			IRuntimeGameObject effectOwner, 
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			return InternalCreateAndExecuteEffect(effectConfigId, effectOwner, false, args, targets);
		}

		public IRuntimeEffect CreateAndExecuteEffectAuto(
			string effectConfigId, 
			IRuntimeGameObject effectOwner, 
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			var effectData = gameContext.GameDatabase.GetEffectConfig(effectConfigId);
			var autoAssignTargets = gameLogicContext.TargetResolver.GetAutoTargets(effectData, effectOwner, targets);
			return InternalCreateAndExecuteEffect(effectConfigId, effectOwner, false, args, autoAssignTargets);
		}

		public void ExecutePhase(PhaseInfo info)
		{
			if (info.Phase == EffectPhase.None
			    || info.Executor.HasEffectsDisable())
				return;
			
			// execute/create all applied effects
			var executed = InternalExecuteAppliedEffectsByPhase(info).Select(x=> x.EffectData.Id).ToList();
			
			// filter effects
			var phaseEffects = gameContext.GameDatabase
				.GetEffects(info.Executor.RuntimeData.ImposingEffects)
				.Where(effectData => !executed.Contains(effectData.Id) && !effectData.Applied && FilterEffectByPhase(effectData, info))
				.ToArray();
			
			if (!phaseEffects.Any())
				return;

			var phaseManualTargetEffect = phaseEffects
				.Where(effectData => effectData.TargetMod == EffectTargetMod.PlayerPicked)
				.ToArray();
			
			if (phaseManualTargetEffect.Any())
			{
				if (gameLogicContext.TargetResolver.HasManualTargets(info.Executor.RuntimeData.Id))
				{
					var manualTargets = gameLogicContext.TargetResolver.GetManualTargets(info.Executor.RuntimeData.Id);
					foreach (var effectData in phaseManualTargetEffect)
					{
						InternalCreateAndExecuteEffect(effectData.Id, info.Executor, true, null, manualTargets);
					}
				}
			}

			var phaseAutoTargetEffects = phaseEffects.Where(effectData => effectData.TargetMod != EffectTargetMod.PlayerPicked);

			foreach (var effectData in phaseAutoTargetEffects)
			{
				var autoAssignTargets = gameLogicContext.TargetResolver.GetAutoTargets(effectData, info.Executor, info.Targets);
				InternalCreateAndExecuteEffect(effectData.Id, info.Executor, true, null, autoAssignTargets);
			}
		}
		
		public void ExpireAppliedEffects(IRuntimeGameObject executor, ExpirePhase expirePhase)
		{
			foreach (var effect in executor.AppliedEffects.ToArray())
				TryExpireEffect(expirePhase, effect, executor);
		}
		
		public bool ExecuteEffect(IRuntimeEffect effect)
		{
			if (!effect.CanExecute())
				return false;

			var startEffect = new StartEffect(effect.RuntimeData);
			// override the effect targets for clients, only for targets on which the effect will work.
			startEffect.RuntimeData.TargetIds = effect.GetExecutionTargets().Select(x => x.RuntimeData.Id).ToList();
			gameLogicContext.LogicQueueController.Add(startEffect, effect.GetAccessibleReceiver());

			effect.Execute();

			var endEffect = new EndEffect(effect.RuntimeData);
			// override the effect targets for clients, only for targets on which the effect will work.
			endEffect.RuntimeData.TargetIds = effect.GetExecutionTargets().Select(x => x.RuntimeData.Id).ToList();
			gameLogicContext.LogicQueueController.Add(endEffect, effect.GetAccessibleReceiver());

			effect.OnExecuted();
			return true;
		}
		
		public bool ExecuteAppliedEffect(IRuntimeEffect effect)
		{
			var autoAssignTargets = gameLogicContext.TargetResolver
				.GetAutoTargets(effect.EffectData, effect.Executor, effect.Targets);
				
			effect.SetTargets(autoAssignTargets);
			if (!ExecuteEffect(effect)) 
				return false;
			
			TryExpireEffect(ExpirePhase.Execute, effect, effect.Executor);
			return true;

		}
		
		private IRuntimeEffect InternalCreateAppliedEffectFor(
			string effectConfigId, 
			IRuntimeGameObject effectOwner,
			bool byPhase = false,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			var runtimeEffect = gameLogicContext.RuntimeFactory.CreateRuntimeEffect(effectConfigId, effectOwner, args, false, targets);
			return InternalAddOrStackAppliedEffect(runtimeEffect, effectOwner, byPhase);
		}
		
		private IRuntimeEffect InternalCreateAndExecuteEffect(
			string effectConfigId, 
			IRuntimeGameObject effectOwner, 
			bool byPhase = false,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			if (effectOwner.TryGetAppliedEffect(effectConfigId, out var effect))
				return InternalCreateAppliedEffectFor(effectConfigId, effectOwner, byPhase, args, targets);
			
			effect = gameLogicContext.RuntimeFactory.CreateRuntimeEffect(effectConfigId, effectOwner, args, false, targets);
			if (ExecuteEffect(effect))
				TryExpireEffect(ExpirePhase.Execute, effect, effectOwner);
			return effect;
		}

		private IRuntimeEffect InternalAddOrStackAppliedEffect(
			IRuntimeEffect newEffect, 
			IRuntimeGameObject effectOwner, 
			bool byPhase = false)
		{
			if (newEffect.RuntimeData.CurrentLength == 0)
			{
				DefaultSharedLogger.Log($"To add an effect {newEffect.EffectData.Keyword}, the effect length must be greater than 0");
				return newEffect;
			}
			
			effectOwner.AddOrStackAppliedEffect(newEffect, out var stackEffect, out var executeEffect);

			if (!newEffect.EffectData.FirstTickExecute && !byPhase)
				return stackEffect;
			
			if (ExecuteEffect(executeEffect))
				TryExpireEffect(ExpirePhase.Execute, executeEffect, effectOwner);
			
			return stackEffect;
		}

		private IList<IRuntimeEffect> InternalExecuteAppliedEffectsByPhase(PhaseInfo info)
		{
			var executedApplied = InternalCreateAndExecuteAppliedEffectsByPhase(info);
			var notExecuted = info.Executor.AppliedEffects
				.Where(x => !x.RuntimeData.Disabled)
				.Where(x => !executedApplied.Contains(x) && FilterEffectByPhase(x.EffectData, info, true))
				.ToArray();

			foreach (var effect in notExecuted)
			{
				executedApplied.Add(effect);
				var autoAssignTargets = gameLogicContext.TargetResolver
					.GetAutoTargets(effect.EffectData, info.Executor, info.Targets);
				
				effect.SetTargets(autoAssignTargets);
				if (ExecuteEffect(effect))
					TryExpireEffect(ExpirePhase.Execute, effect, info.Executor);
			}
			
			return executedApplied;
		}

		private IList<IRuntimeEffect> InternalCreateAndExecuteAppliedEffectsByPhase(PhaseInfo info)
		{
			return gameContext.GameDatabase
				.GetEffects(info.Executor.RuntimeData.ImposingEffects)
				.Where(effectData => effectData.Applied && FilterEffectByPhase(effectData, info, true))
				.Select(effectData => InternalCreateAndExecuteAppliedEffectFromImposingEffectAuto(effectData.Id, info.Executor, true, null, info.Targets)).ToList();
		}

		private IRuntimeEffect InternalCreateAndExecuteAppliedEffectFromImposingEffect(
			string effectConfigId, 
			IRuntimeGameObject effectOwner,
			bool byPhase = false,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			effectOwner.RemoveImpossingEffects(effectConfigId);
			var runtimeEffect = gameLogicContext.RuntimeFactory.CreateRuntimeEffect(effectConfigId, effectOwner, args, true, targets);
			return InternalAddOrStackAppliedEffect(runtimeEffect, effectOwner, byPhase);
		}

		private IRuntimeEffect InternalCreateAndExecuteAppliedEffectFromImposingEffectAuto(
			string effectConfigId,
			IRuntimeGameObject effectOwner,
			bool byPhase = false,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(effectConfigId);
			var autoAssignTargets = gameLogicContext.TargetResolver.GetAutoTargets(effectConfig, effectOwner, targets);
			return InternalCreateAndExecuteAppliedEffectFromImposingEffect(effectConfigId, effectOwner, byPhase, args, autoAssignTargets);
		}

		private bool FilterEffectByPhase(EffectData effectData, PhaseInfo info, bool exceptPlayerPick = false)
		{
			return effectData != null
			       && (!exceptPlayerPick || effectData.TargetMod != EffectTargetMod.PlayerPicked)
			       && effectData.Phases.Contains(info.Phase)
			       && CheckEffectPhaseLimit(effectData, info)
			       && CheckEffectPhaseDamageLimit(effectData, info)
			       && CanEffectPhaseTrigger(effectData, info);
		}
		
		private void TryExpireEffect(ExpirePhase expirePhase, IRuntimeEffect effect, IRuntimeGameObject effectOwner)
		{
			if (effect?.RuntimeData is not {CurrentLength: > -1} 
			    || effect.RuntimeData is not {DisabledLength: > -1} 
			    || !effect.EffectData.ExpirePhases.Contains(expirePhase))
				return;

			if (effect.RuntimeData.Disabled)
			{
				effect.RuntimeData.DisabledLength = Math.Max(0, effect.RuntimeData.DisabledLength - 1);
				return;
			}

			effect.RuntimeData.CurrentLength = Math.Max(0, effect.RuntimeData.CurrentLength - 1);
			effect.OnExpirePhase();
			if (effect.RuntimeData.CurrentLength != 0)
				return;

			if (!effectOwner.RemoveAppliedEffect(effect)) // when false, expire non applied effect
				effect.Expire();
		}

		private bool CanEffectPhaseTrigger(EffectData data, PhaseInfo info)
		{
			if (data == null || info.Executor == null || info.Initiator == null)
				return false;

			if (data.PhaseTriggers.Contains(EffectPhaseTrigger.AnySide))
				return true;

			return data.PhaseTriggers.All(trigger => trigger switch
			{
				EffectPhaseTrigger.AnySide => true,
				EffectPhaseTrigger.AlliedSide => info.Initiator.RuntimeData.OwnerUserId == info.Executor.RuntimeData.OwnerUserId,
				EffectPhaseTrigger.EnemySide => info.Initiator.RuntimeData.OwnerUserId != info.Executor.RuntimeData.OwnerUserId,
				EffectPhaseTrigger.TurnSide => info.Initiator.RuntimeData.OwnerUserId == gameContext.Timer.RuntimeData.OwnerId,
				
				EffectPhaseTrigger.ExceptSelf => info.Initiator.RuntimeData.Id != info.Executor.RuntimeData.Id,
				EffectPhaseTrigger.OnlySelf => info.Initiator.RuntimeData.Id == info.Executor.RuntimeData.Id,
				
				EffectPhaseTrigger.BuildingType => info.Initiator.Data.Type == ObjectType.Building,
				EffectPhaseTrigger.CreatureType => info.Initiator.Data.Type == ObjectType.Creature,
				EffectPhaseTrigger.SpellType => info.Initiator.Data.Type == ObjectType.Spell,
				EffectPhaseTrigger.HeroType => info.Initiator.Data.Type == ObjectType.Hero,
	
				_ => throw new NotImplementedException($"[{GetType().Name}] Unknown {nameof(EffectPhaseTrigger)} : {data.PhaseTriggers}")
			});
		}

		private bool CheckEffectPhaseLimit(EffectData data, PhaseInfo info)
		{
			if (data.PhaseLimits is not {Length: > 0})
				return true;

			var currState = info.Executor switch
			{
				IRuntimeGameCard gameCard => gameCard.RuntimeData.State,
				_ => RuntimeState.InTable
			};

			return data.PhaseLimits.Contains(currState);
		}

		private bool CheckEffectPhaseDamageLimit(EffectData data, PhaseInfo info)
		{
			return data.PhaseDamageLimits.AnyExcept(DamageType.ExFlag, DamageType.None, info.DamageType);
		}
	}
}
