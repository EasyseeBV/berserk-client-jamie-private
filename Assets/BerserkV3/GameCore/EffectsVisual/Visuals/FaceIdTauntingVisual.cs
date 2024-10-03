using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.LogicEventsProcessor;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Reflect)]
	[EffectVisual(EffectVisualKeyword.Stunning)]
	[EffectVisual(EffectVisualKeyword.Immortal)]
	[EffectVisual(EffectVisualKeyword.ImmuneToPoisoning)]
	[EffectVisual(EffectVisualKeyword.ImmuneToSleeping)]
	public class FaceIdTauntingVisual : FaceIdVisual
	{
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameDatabase gameDatabase;
		private readonly List<int> tauntIds = new();
		private CancellationTokenSource lifeTime;
		
		public FaceIdTauntingVisual(
			IGameLogicEventsSource gameLogicEventsSource,
			IGameDatabase gameDatabase)
		{
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameDatabase = gameDatabase;
		}

		public override async UniTask ApplyLongEffectAsync()
		{
			lifeTime?.Cancel();
			lifeTime?.Dispose();
			lifeTime = new CancellationTokenSource();
			gameLogicEventsSource.Subscribe<AddObjectEffect>(OnAppliedEffectAddedAsync, lifeTime.Token);
			gameLogicEventsSource.Subscribe<DeleteObjectEffect>(OnAppliedEffectDeletedAsync, lifeTime.Token);
			
			await UniTask.WhenAll(Executor.RuntimeGameObject.AppliedEffects
				.OrderBy(x => (int)x.EffectData.VisualKeyword)
				.Select(effect => RefreshTauntingModeAsync(true, effect.RuntimeData))
				.Append(base.ApplyLongEffectAsync()));
		}

		public override UniTask ExpireLongEffectAsync()
		{
			lifeTime?.Cancel();
			lifeTime?.Dispose();
			lifeTime = null;
			tauntIds.Clear();

			return TryGetLayout(Executor, out var layout) 
				? UniTask.WhenAll(layout.RemoveFaceAsync(GetFaceTauntedId()), base.ExpireLongEffectAsync()) 
				: UniTask.CompletedTask;
		}

		private UniTask RefreshTauntingModeAsync(bool enabled, IRuntimeEffectData runtimeEffectData)
		{
			if (lifeTime == null)
				return UniTask.CompletedTask;
			
			if (runtimeEffectData.Id == Model.Id)
				return UniTask.CompletedTask;

			var hasEffectId = tauntIds.Contains(runtimeEffectData.Id);
			if ((enabled && hasEffectId)
			    || (!enabled && !hasEffectId))
				return UniTask.CompletedTask;
			
			var effectData = gameDatabase.GetEffectConfig(runtimeEffectData.ConfigId);
			if (effectData.Keyword != EffectKeyword.Taunting)
				return UniTask.CompletedTask;

			if (enabled)
				tauntIds.Add(runtimeEffectData.Id);
			else 
				tauntIds.Remove(runtimeEffectData.Id);

			if ((enabled && tauntIds.Count > 1)
				||(!enabled && tauntIds.Count >= 1))
				return UniTask.CompletedTask;

			return TryGetLayout(Executor, out var layout) 
				? UniTask.WhenAll(enabled 
					? layout.AddFaceAsync(GetFaceTauntedId())
					: layout.RemoveFaceAsync(GetFaceTauntedId())) 
				: UniTask.CompletedTask;
		}

		private UniTask OnAppliedEffectAddedAsync(AddObjectEffect data)
		{
			if (lifeTime == null)
				return UniTask.CompletedTask;
			
			return Model.ExecutorId != data.RuntimeData.ExecutorId
				? UniTask.CompletedTask 
				: RefreshTauntingModeAsync(true, data.RuntimeData);
		}

		private UniTask OnAppliedEffectDeletedAsync(DeleteObjectEffect data)
		{
			if (lifeTime == null)
				return UniTask.CompletedTask;
			
			return Model.ExecutorId != data.RuntimeData.ExecutorId
				? UniTask.CompletedTask 
				: RefreshTauntingModeAsync(false, data.RuntimeData);
		}

		protected virtual FaceId GetFaceTauntedId()
		{
			var baseFaceId = GetFaceId();
			var tauntedId = EffectVisualKeyword.Taunting;
			var order = Mathf.Max((int) tauntedId, baseFaceId.Order) + 1;
			return new FaceId($"{baseFaceId.Key}_{tauntedId}", order);
		}
	}
}