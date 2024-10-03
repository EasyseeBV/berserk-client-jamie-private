using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Models;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Repository;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BerserkV3.GameCore.EffectsVisual.Applications
{
	public class EffectsApplication : DisposableWithCts, IInitializable
	{
		private readonly IGameContext gameContext;
		private readonly IGameRepository gameRepository;
		private readonly IRuntimeEffectModelFatory effectModelFatory;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IVisualSequenceApplication visualSequenceApplication;

		public EffectsApplication(
			IGameContext gameContext,
			IGameRepository gameRepository,
			IRuntimeEffectModelFatory effectModelFatory,
			IGameLogicEventsSource gameLogicEventsSource,
			IVisualSequenceApplication visualSequenceApplication)
		{
			this.gameContext = gameContext;
			this.gameRepository = gameRepository;
			this.effectModelFatory = effectModelFatory;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.visualSequenceApplication = visualSequenceApplication;
		}

		public void Initialize()
		{
			gameLogicEventsSource.Subscribe<StartEffect>(StartEffectAsync, Token);
			gameLogicEventsSource.Subscribe<EndEffect>(EndEffectAsync, Token);
			gameLogicEventsSource.Subscribe<AddObjectEffect>(AddEffectAsync, Token);
			gameLogicEventsSource.Subscribe<ChangeObjectEffect>(ChachgeEffectAsync, Token);
			gameLogicEventsSource.Subscribe<DeleteObjectEffect>(DeleteEffectAsync, Token);
			
			gameLogicEventsSource.Subscribe<ObjectHitEvent>(data => PlayCustomAsync(data.RuntimeArg.RuntimeId, EffectVisualKeyword.TakeHit, data.RuntimeArg), Token);
			gameLogicEventsSource.Subscribe<ObjectRestoreEvent>(data => PlayCustomAsync(data.RuntimeArg.RuntimeId, EffectVisualKeyword.TakeHeal, data.RuntimeArg), Token);
			gameLogicEventsSource.Subscribe<ObjectDestructionEvent>(data => PlayCustomAsync(data.RuntimeId, GetDestructionKeyword(data.RuntimeId)), Token);
			gameLogicEventsSource.Subscribe<ObjectSpawnEvent>(data => PlayCustomAsync(data.RuntimeId, EffectVisualKeyword.CardSpawn), Token);
		}
	
		private EffectVisualKeyword GetDestructionKeyword(int runtimeId)
		{
			try
			{
				var executor = gameRepository.GetObjectViewByRuntimeId(runtimeId);
				return executor.RuntimeData is IRuntimeCardData {IsToken: true}
					? EffectVisualKeyword.Exile
					: EffectVisualKeyword.Destruction;
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return EffectVisualKeyword.Destruction;
			}
		}
		
		private UniTask PlayCustomAsync(int runtimeId, EffectVisualKeyword keyword, params IEffectRuntimeArg[] args)
		{
			try
			{
				var effectData = new EffectDataMock(keyword);
				var executor = gameRepository.GetObjectViewByRuntimeId(runtimeId);
				var model = effectModelFatory.Create(effectData, executor, args, executor);
				return visualSequenceApplication.PlaySingleSequenceAsync(model);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return UniTask.CompletedTask;
			}
		}

		private UniTask StartEffectAsync(StartEffect startEffect)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(startEffect.RuntimeData.ConfigId);
			if (effectConfig == null || effectConfig.VisualKeyword == EffectVisualKeyword.None)
				return UniTask.CompletedTask;

			var model = effectModelFatory.Create(startEffect.RuntimeData);
			return visualSequenceApplication.StartEffectAsync(model);
		}

		private UniTask EndEffectAsync(EndEffect endEffect)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(endEffect.RuntimeData.ConfigId);
			if (effectConfig == null || effectConfig.VisualKeyword == EffectVisualKeyword.None)
				return UniTask.CompletedTask;

			var model = effectModelFatory.Create(endEffect.RuntimeData);
			return visualSequenceApplication.EndEffectAsync(model);
		}

		private UniTask AddEffectAsync(AddObjectEffect addObjectEffect)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(addObjectEffect.RuntimeData.ConfigId);
			if (effectConfig == null || effectConfig.VisualKeyword == EffectVisualKeyword.None)
				return UniTask.CompletedTask;

			var model = effectModelFatory.Create(addObjectEffect.RuntimeData);
			return visualSequenceApplication.ApplyLongEffectAsync(model, false);
		}

		private UniTask ChachgeEffectAsync(ChangeObjectEffect changeObjectEffect)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(changeObjectEffect.RuntimeData.ConfigId);
			if (effectConfig == null || effectConfig.VisualKeyword == EffectVisualKeyword.None)
				return UniTask.CompletedTask;

			var model = effectModelFatory.Create(changeObjectEffect.RuntimeData);
			return visualSequenceApplication.ChangeEffectAsync(model);
		}

		private UniTask DeleteEffectAsync(DeleteObjectEffect deleteEffect)
		{
			var effectConfig = gameContext.GameDatabase.GetEffectConfig(deleteEffect.RuntimeData.ConfigId);
			if (effectConfig == null || effectConfig.VisualKeyword == EffectVisualKeyword.None)
				return UniTask.CompletedTask;

			var model = effectModelFatory.Create(deleteEffect.RuntimeData);
			return visualSequenceApplication.ExpireLongEffectAsync(model);
		}
	}
}