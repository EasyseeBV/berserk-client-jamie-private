using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers;
using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.RuntimeObjects;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.LogicContext
{
	public interface IRuntimeFactory
	{
		IRuntimeGameCard CreateRuntimeCard(string configId, string userId, bool subscribe = true);
		IRuntimeHero CreateRuntimeHero(string configId, string userId, bool subscribe = true);
		IRuntimeGameObject CreateRuntimeObject(IRuntimeData runtimeData, bool subscribe = true);

		IRuntimeEffect CreateRuntimeEffectAutoTargets(
			string effectConfigId,
			IRuntimeGameObject executor,
			bool isInnate,
			IEnumerable<IEffectRuntimeArg> args = null);

		IRuntimeEffect CreateRuntimeEffect(
			string effectConfigId,
			IRuntimeGameObject executor,
			IEnumerable<IEffectRuntimeArg> args = null,
			bool isInnate = false,
			params IRuntimeGameObject[] targets);

		IRuntimeEffect CreateRuntimeEffect(IRuntimeEffectData runtimeData);

		IEffectCondition CreateCondition(EffectConditionType type);
	}
	
	public class RuntimeFactory : IRuntimeFactory
	{
		private readonly IGameContext gameContext;
		private readonly IGameLogicContext gameLogicContext;
		private IEffectsFactory EffectsFactory => gameLogicContext.EffectsFactory;
		private IEffectPhaseProcessor EffectPhaseProcessor => gameLogicContext.EffectPhaseProcessor;
		private IRuntimeStateController StateController => gameLogicContext.RuntimeStateController;
		private IRuntimeCardPositionController CardPositionController => gameLogicContext.RuntimeCardPositionController;
		private IRuntimeIdGenerator RuntimeIdGenerator => gameContext.RuntimeIdGenerator;

		public RuntimeFactory(
			IGameContext gameContext,
			IGameLogicContext gameLogicContext)
		{
			this.gameContext = gameContext;
			this.gameLogicContext = gameLogicContext;
		}

		public IRuntimeGameCard CreateRuntimeCard(string configId, string userId, bool subscribe = true)
		{
			var runtimeData = CreateRuntimeData(configId, ObjectType.CardsMask, userId);
			var runtimeObject = CreateRuntimeObject(runtimeData, subscribe);
			return (IRuntimeGameCard) runtimeObject;
		}

		public IRuntimeHero CreateRuntimeHero(string configId, string userId, bool subscribe = true)
		{
			var runtimeData = CreateRuntimeData(configId, ObjectType.Hero, userId);
			var runtimeObject = CreateRuntimeObject(runtimeData, subscribe);
			return (IRuntimeHero) runtimeObject;
		}

		public IRuntimeGameObject CreateRuntimeObject(IRuntimeData runtimeData, bool subscribe = true)
		{
			if (runtimeData == null)
				throw new NullReferenceException($"[{GetType().Name}] {nameof(runtimeData)} is null.");

			if (gameContext.GameRuntimePool.TryGet(runtimeData.Id, out var runtimeObject))
				return runtimeObject;

			var objectData = GetObjectData(runtimeData.DataId, runtimeData.Type);
			runtimeObject = runtimeData.Type switch
			{
				ObjectType.Building => new RuntimeBuildingCard().Init(runtimeData, objectData),
				ObjectType.Creature => new RuntimeGameCard().Init(runtimeData, objectData),
				ObjectType.Spell => new RuntimeSpellCard().Init(runtimeData, objectData),
				ObjectType.Hero => new RuntimeHero().Init(runtimeData, objectData),
				_ => throw new NotImplementedException($"[{GetType().Name}] Unknown {nameof(ObjectType)} : {runtimeData.Type}")
			};

			gameContext.GameRuntimePool.Add(runtimeObject);
			
			if (subscribe)
				SubscribeRuntimeObject(runtimeObject);
			
			return runtimeObject;
		}

		public IRuntimeEffect CreateRuntimeEffectAutoTargets(
			string effectConfigId,
			IRuntimeGameObject executor,
			bool isInnate,
			IEnumerable<IEffectRuntimeArg> args = null)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(effectConfigId);
			var autoAssignTargets = gameLogicContext.TargetResolver.GetAutoTargets(effectConfig, executor);
			return InternalCreateRuntimeEffect(effectConfig, executor, args, isInnate, autoAssignTargets);
		}

		public IRuntimeEffect CreateRuntimeEffect(IRuntimeEffectData runtimeData)
		{
			var effectData = gameContext.GameDatabase.GetEffectConfig(runtimeData.ConfigId);
			var executor = gameContext.GameRuntimePool.Get(runtimeData.ExecutorId);
			var targets = gameContext.GameRuntimePool.GetMany(runtimeData.TargetIds).ToArray();
			return EffectsFactory.Create(effectData.Keyword)
				.Init(effectData, runtimeData, gameLogicContext, gameContext, executor, targets);
		}

		public IRuntimeEffect CreateRuntimeEffect(
			string effectConfigId,
			IRuntimeGameObject executor,
			IEnumerable<IEffectRuntimeArg> args = null,
			bool isInnate = false,
			params IRuntimeGameObject[] targets)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(effectConfigId);
			return InternalCreateRuntimeEffect(effectConfig, executor, args, isInnate, targets);
		}

		public IEffectCondition CreateCondition(EffectConditionType type)
		{
			return EffectsFactory.Create(type);
		}

		private IRuntimeData CreateRuntimeData(string configId, ObjectType type, string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new NullReferenceException($"[{GetType().Name}] {nameof(userId)} for [{nameof(ObjectType)} : {type}] is null or empty.");

			var objectData = GetObjectData(configId, type);
			return type switch
			{
				ObjectType.CardsMask => new RuntimeCardData(objectData) {OwnerUserId = userId, Id = RuntimeIdGenerator.Next()},
				ObjectType.Hero => new RuntimeHeroData(objectData) {OwnerUserId = userId, Id = RuntimeIdGenerator.Next()},
				_ => throw new NotImplementedException($"[{GetType().Name}] Unknown {nameof(ObjectType)} : {type}")
			};
		}

		private IObjectData GetObjectData(string configId, ObjectType type)
		{
			if (string.IsNullOrEmpty(configId))
				throw new NullReferenceException($"[{GetType().Name}] {nameof(configId)} for [{nameof(ObjectType)} : {type}] is null or empty.");

			IObjectData objectData = type switch
			{
				ObjectType.Building or ObjectType.Creature or ObjectType.Spell or ObjectType.CardsMask
					=> gameContext.GameDatabase.GetCard(configId),

				ObjectType.Hero => gameContext.GameDatabase.GetHero(configId),
				_ => throw new NotImplementedException($"[{GetType().Name}] Unknown {nameof(ObjectType)} : {type}")
			};

			if (objectData == null)
				throw new NullReferenceException($"[{GetType().Name}] {nameof(objectData)} with [{nameof(configId)} : {configId}] is null.");

			return objectData;
		}

		private void SubscribeRuntimeObject(IRuntimeGameObject runtimeObject)
		{
			if (runtimeObject == null)
				throw new NullReferenceException($"[{GetType().Name}] {nameof(IRuntimeGameObject)} is null.");

			switch (runtimeObject)
			{
				case IRuntimeGameCard gameCard:
					StateController.Process(gameCard);
					CardPositionController.Process(gameCard);
					EffectPhaseProcessor.HandleRuntimeObject(gameCard);
					break;

				case IRuntimeHero runtimeHero:
					EffectPhaseProcessor.HandleRuntimeObject(runtimeHero);
					break;

				default:
					throw new NotImplementedException($"[{GetType().Name}] Unknown object type : {runtimeObject}");
			}
		}

		private IRuntimeEffect InternalCreateRuntimeEffect(
			EffectData effectConfig,
			IRuntimeGameObject executor,
			IEnumerable<IEffectRuntimeArg> args = null,
			bool isInnate = false,
			params IRuntimeGameObject[] targets)
		{

			var runtimeData = new RuntimeEffectData
			{
				Id = RuntimeIdGenerator.Next(),
				ConfigId = effectConfig.Id,
				IsInnate = isInnate,
				ExecutorId = executor.RuntimeData.Id,
				ExecutorOwnerId = executor.RuntimeData.OwnerUserId,
				CurrentLength = effectConfig.Length,
				CurrentValue = effectConfig.Value,
				TargetIds = targets.Select(x => x.RuntimeData.Id).ToList(),
				RuntimeArgs = args?.ToList() ?? new List<IEffectRuntimeArg>(),
				AccessLevel = executor.GetAccessLevel(),
				ExecutionOrder = effectConfig.ExecutionOrder
			};
			
			var keywordEffect = EffectsFactory
				.Create(effectConfig.Keyword)
				.Init(effectConfig, runtimeData, gameLogicContext, gameContext, executor, targets);

			if (!effectConfig.FirstTickApply && !effectConfig.FirstTickExecute && effectConfig.Length == 0)
				throw new NullReferenceException(
					$"{keywordEffect}: {nameof(effectConfig.FirstTickApply)} == {effectConfig.FirstTickApply}" +
					$"{nameof(effectConfig.Length)} must not be shorter than 0 in data.");

			keywordEffect.Create();
			return keywordEffect;
		}
	}
}