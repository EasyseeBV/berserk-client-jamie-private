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

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	public abstract class KeywordEffect : IRuntimeEffect
	{
		public IRuntimeGameObject[] Targets { get; private set; }
		public IRuntimeGameObject Executor { get; private set; }
		public EffectData EffectData { get; private set; }
		public IRuntimeEffectData RuntimeData { get; private set; }

		protected IGameContext GameContext { get; private set; }
		protected IGameLogicContext LogicContext { get; private set; }

		public KeywordEffect Init(
			EffectData effectData,
			IRuntimeEffectData runtimeData,
			IGameLogicContext logicContext,
			IGameContext gameContext,
			IRuntimeGameObject executor,
			params IRuntimeGameObject[] targers)
		{
			RuntimeData = runtimeData;
			EffectData = effectData;
			GameContext = gameContext;
			LogicContext = logicContext;
			Executor = executor;
			Targets = targers;
			OnInit();
			return this;
		}

		public IRuntimeEffect Sync(IRuntimeEffectData runtimeData)
		{
			RuntimeData = runtimeData;
			EffectData = GameContext.GameDatabase.GetEffectConfig(RuntimeData.ConfigId);
			Executor = GameContext.GameRuntimePool.Get(RuntimeData.ExecutorId);
			SetTargets(GameContext.GameRuntimePool.GetMany(RuntimeData.TargetIds).ToArray());
			return this;
		}

		/// <summary>
		///     Place all one-time logic here.
		/// </summary>
		public virtual void Create()
		{
		}

		public void Expire()
		{
			var batchIdExpire = Math.Max(1, RuntimeData.Id) * 2;
			TryMarkEffectBatching(batchIdExpire);
			OnExpire();
			foreach (var target in GetExecutionTargets())
				target.RemoveAppliedEffect(this);

			Executor?.RemoveAppliedEffect(this);
			RuntimeData.AppliedIds.Clear();
			RuntimeData.CurrentLength = 0;
			RuntimeData.DisabledLength = 0;
			TryMarkEffectBatching(batchIdExpire, false);
		}

		/// <summary>
		///     Called each time the effect is triggered.
		/// </summary>
		public void Execute()
		{
			var batchIdExecute = Math.Max(1, RuntimeData.Id) + 1;
			TryMarkEffectBatching(batchIdExecute);
			OnExecute();
			TryMarkEffectBatching(batchIdExecute, false);
		}

		public virtual bool CanExecute()
		{
			return EffectData.MinTargetCount <= 0 || (Targets != null && Targets.Length >= EffectData.MinTargetCount);
		}

		public void SetTargets(IRuntimeGameObject[] targets)
		{
			Targets = targets;
			RuntimeData.TargetIds = Targets.Select(x => x.RuntimeData.Id).ToList();
		}

		public void Disable(int length)
		{
			RuntimeData.DisabledLength = length;
			OnDisabled();

			if (EffectData.Applied)
				Executor.ChangedAppliedEffect(this);
		}

		public virtual void OnExpirePhase()
		{
		}

		public virtual void OnExecuted()
		{
		}

		public virtual void OnDeleted()
		{
		}

		public virtual void OnChanged()
		{
		}

		public virtual void OnAdded()
		{
		}

		protected virtual void OnDisabled()
		{
		}

		public virtual IRuntimeEffect Stack(IRuntimeEffect other)
		{
			foreach (var effectStack in EffectData.EffectStacks)
				InternalStack(effectStack, other);

			RuntimeData.RuntimeArgs.AddRange(other.RuntimeData.RuntimeArgs);
			RuntimeData.AppliedIds = RuntimeData.AppliedIds
				.Union(other.RuntimeData.AppliedIds)
				.Distinct()
				.ToList();

			SetTargets(Targets.Union(other.Targets).Distinct().ToArray());
			return OnStacked(other);
		}

		private void InternalStack(EffectStack value, IRuntimeEffect other)
		{
			switch (value)
			{
				case EffectStack.AdditiveValue:
					RuntimeData.CurrentValue += other.RuntimeData.CurrentValue;
					break;

				case EffectStack.AdditiveLength:
					RuntimeData.CurrentLength = StackLengthAdditive(other.RuntimeData.CurrentLength);
					break;

				case EffectStack.AdditiveBoth:
					RuntimeData.CurrentValue += other.RuntimeData.CurrentValue;
					RuntimeData.CurrentLength = StackLengthAdditive(other.RuntimeData.CurrentLength);
					break;

				case EffectStack.Custom:
					OnStackCustom(other);
					break;

				case EffectStack.ReplaceValue:
					RuntimeData.CurrentValue = other.RuntimeData.CurrentValue;
					break;

				case EffectStack.ReplaceLength:
					RuntimeData.CurrentLength = other.RuntimeData.CurrentLength;
					break;

				case EffectStack.ReplaceBoth:
					RuntimeData.CurrentValue = other.RuntimeData.CurrentValue;
					RuntimeData.CurrentLength = other.RuntimeData.CurrentLength;
					break;

				case EffectStack.ReplaceMoreLength:
					RuntimeData.CurrentLength = Math.Max(RuntimeData.CurrentLength, other.RuntimeData.CurrentLength);
					break;
				case EffectStack.ReplaceMoreValue:
					RuntimeData.CurrentValue = Math.Max(RuntimeData.CurrentValue, other.RuntimeData.CurrentValue);
					break;
				case EffectStack.ReplaceMoreBoth:
					RuntimeData.CurrentLength = Math.Max(RuntimeData.CurrentLength, other.RuntimeData.CurrentLength);
					RuntimeData.CurrentValue = Math.Max(RuntimeData.CurrentValue, other.RuntimeData.CurrentValue);
					break;
				case EffectStack.DoNotStack:
				default: throw new ArgumentOutOfRangeException($"{value} - Cant use stack");
			}

			return;

			int StackLengthAdditive(int otherValue)
			{
				if (otherValue < 0)
					return RuntimeData.CurrentLength;

				if (RuntimeData.CurrentLength < 0)
					return otherValue;

				return RuntimeData.CurrentLength + otherValue;
			}
		}

		public virtual IRuntimeGameObject[] GetExecutionTargets()
		{
			return GetExecutionTargetsInternal(EffectData.ExecuteTargets);
		}

		public virtual void Dispose()
		{
			//Manual cleanup works too early, breaks correct sync with client. TODO: find problem
			//Executor = null;
			//Targets = null;
		}

		/// <summary>
		///     Called each time the effect recovery and init
		/// </summary>
		protected virtual void OnInit()
		{
		}

		/// <summary>
		///     Called each time the effect is triggered.
		/// </summary>
		protected virtual void OnExecute()
		{
		}

		protected virtual void OnExpire()
		{
		}

		protected virtual void OnStackCustom(IRuntimeEffect other)
		{
			DefaultSharedLogger.Error("Stack custom is enabled but, not implemented!");
		}

		/// <summary>
		/// What effect should execute after the stack
		/// </summary>
		/// <param name="other">effect which stack with this effect</param>
		/// <returns>What effect should execute after the stack</returns>
		protected virtual IRuntimeEffect OnStacked(IRuntimeEffect other)
		{
			return other;
		}

		protected virtual int ValueModRounded(IntStat stat = null, EffectValue? effectValue = null)
		{
			var rawValue = ValueMode(stat, effectValue);
			return (int) Math.Round(rawValue);
		}

		protected float ValueMode(IntStat stat = null, EffectValue? effectValue = null)
		{
			var ownerUserId = Executor.RuntimeData.OwnerUserId;
			var ownerContex = GameContext.PlayerRepository.Get(ownerUserId);
			var opposContex = GameContext.PlayerRepository.GetOpposite(ownerUserId);
			effectValue ??= EffectData.ValueMod;

			return effectValue switch
			{
				EffectValue.Integer => RuntimeData.CurrentValue,

				EffectValue.PercentFromCurrent => CalculatePercentFromCurrent(stat),

				EffectValue.PercentFromMaximum => CalculatePercentFromMaximum(stat),

				EffectValue.AlliedCount => Math.Max(0, GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InTable, ownerUserId, ObjectType.TableCardsMask)
					.Count() * RuntimeData.CurrentValue),

				EffectValue.AlliedHandCount => Math.Max(0, GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InHand, ownerUserId)
					.Count() * RuntimeData.CurrentValue),

				EffectValue.AlliedCountExceptSelf => Math.Max(0, (GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InTable, ownerUserId, ObjectType.TableCardsMask)
					.Count() - (Executor.Data.Type.IsTableCard() ? 1 : 0)) * RuntimeData.CurrentValue),

				EffectValue.EnemyCount => Math.Max(0, GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InTable, opposContex.UserId, ObjectType.TableCardsMask)
					.Count() * RuntimeData.CurrentValue),

				EffectValue.TableCardsCount => Math.Max(0, GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InTable, type: ObjectType.TableCardsMask)
					.Count() * RuntimeData.CurrentValue),

				EffectValue.TurnWithoutCards => RuntimeData.CurrentValue * (ownerContex.RuntimeData.TurnsWithoutCards ?? 1),

				EffectValue.TargetCount => Targets is { Length: > 0 } ? RuntimeData.CurrentValue * Targets.Length : 0,

				EffectValue.SelfLava => ownerContex.RuntimeData.Mana * RuntimeData.CurrentValue,

				EffectValue.OpponentLava => opposContex.RuntimeData.Mana * RuntimeData.CurrentValue,

				EffectValue.FreeSelfTableSpace => (GameContext.SharedConfig.MaxCardsOnTable - Math.Max(0, GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InTable, ownerUserId, ObjectType.TableCardsMask)
					.Count())) * RuntimeData.CurrentValue,

				EffectValue.FreeOpponentTableSpace => (GameContext.SharedConfig.MaxCardsOnTable - Math.Max(0, GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InTable, opposContex.UserId, ObjectType.TableCardsMask)
					.Count())) * RuntimeData.CurrentValue,

				_ => throw new NotImplementedException($"Unknown effect value mode : {EffectData.ValueMod}")
			};
		}

		protected virtual void TryMarkEffectBatching(int batchId, bool isStartBatch = true)
		{
			if (EffectData.BatchedVisuals)
				LogicContext.LogicQueueController.Add(new BatchEvent(batchId, isStartBatch));
		}

		protected IRuntimeGameObject[] GetExecutionTargets(params EffectExecuteTarget[] custom)
		{
			return GetExecutionTargetsInternal(custom);
		}

		protected IRuntimeGameObject[] GetExecutionTargetsInternal(IEnumerable<EffectExecuteTarget> flags)
		{
			if (flags == null)
				return Array.Empty<IRuntimeGameObject>();

			return flags.Distinct().SelectMany(flag => flag switch
			{
				EffectExecuteTarget.All => GameContext.GameRuntimePool.GetHeroes()
					.Append(Executor)
					.Distinct() // remove double hero if executor is hero too
					.Union(Targets)
					.Reverse() // targets from base query have to be first in list
					.ToArray(),

				EffectExecuteTarget.Executor => new[] { Executor },
				EffectExecuteTarget.Targets => Targets,
				EffectExecuteTarget.Opposite => new IRuntimeGameObject[]
				{
					GameContext.GameRuntimePool.GetHeroByUserId(GameContext.PlayerRepository.GetOpposite(Executor.RuntimeData.OwnerUserId).UserId)
				},

				EffectExecuteTarget.Self => new IRuntimeGameObject[]
				{
					GameContext.GameRuntimePool.GetHeroByUserId(Executor.RuntimeData.OwnerUserId)
				},
				EffectExecuteTarget.TurnOwner => new IRuntimeGameObject[]
				{
					GameContext.GameRuntimePool.GetHeroes().First(x => x.RuntimeData.OwnerUserId == GameContext.Timer.RuntimeData.OwnerId)
				},
				_ => throw new NotImplementedException($"Unknown {nameof(EffectExecuteTarget)} : {flag}")
			}).ToArray();
		}

		private float CalculatePercentFromCurrent(IntStat stat)
		{
			var statValue = stat ?? 1;
			var currentValue = RuntimeData.CurrentValue;
			var result = statValue * currentValue / 100f;
			var roundedResult = (float)Math.Floor(result);
			return roundedResult;
		}

		private float CalculatePercentFromMaximum(IntStat stat)
		{
			var statTotalMax = stat?.TotalMax ?? 1;
			var currentValue = RuntimeData.CurrentValue;
			var result = statTotalMax * currentValue / 100f;
			var roundedResult = (float)Math.Floor(result);
			return roundedResult;
		}
	}
}