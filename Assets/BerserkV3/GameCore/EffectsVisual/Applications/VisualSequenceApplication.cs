using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Applications
{
	public class VisualSequenceApplication : IVisualSequenceApplication
	{
		private readonly IVisualEffectsFactory visualEffectsFactory;
		private readonly IVisualEffectsRepository visualEffectsRepository;

		public VisualSequenceApplication(
			IVisualEffectsFactory visualEffectsFactory,
			IVisualEffectsRepository visualEffectsRepository)
		{
			this.visualEffectsFactory = visualEffectsFactory;
			this.visualEffectsRepository = visualEffectsRepository;
		}
		
		public async UniTask PlaySingleSequenceAsync(IRuntimeEffectModel model)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None)
				return;
			
			var effectVisual = visualEffectsFactory.Create(model);
			if (effectVisual == null)
			{
				DefaultSharedLogger.Error($"{nameof(PlaySingleSequenceAsync)}.{nameof(effectVisual)} created as null.");
				return;
			}
			
			await effectVisual.PlaySingleEffectAsync();
			effectVisual.Dispose();
		}

		public UniTask StartEffectAsync(IRuntimeEffectModel model)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None 
			    || model.Targets.Length < 1) // TODO Wtf
				return UniTask.CompletedTask;

			if (visualEffectsRepository.TryGet(model.Id, out var effectVisual))
			{
				if (effectVisual != null)
				{
					effectVisual.Sync(model);
					return effectVisual.StartEffectAsync();
				}
				
				visualEffectsRepository.Remove(model.Id);
			}

			effectVisual = visualEffectsFactory.Create(model);
			if (effectVisual == null)
			{
				DefaultSharedLogger.Error($"{nameof(StartEffectAsync)}.{nameof(effectVisual)} created as null.");
				return UniTask.CompletedTask;
			}
			
			visualEffectsRepository.TryAdd(effectVisual);
			return effectVisual.StartEffectAsync();
		}
		
		public async UniTask EndEffectAsync(IRuntimeEffectModel model)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None
			    || !visualEffectsRepository.TryGet(model.Id, out var effectVisual))
				return;

			if (!model.Data.Applied)
				visualEffectsRepository.Remove(model.Id);
			
			if (effectVisual == null)
				return;
			
			effectVisual.Sync(model);
			await effectVisual.EndEffectAsync();
			
			if (!model.Data.Applied)
				effectVisual.Dispose();
		}

		public UniTask ApplyLongEffectAsync(IRuntimeEffectModel model, bool resore)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None
			    || model.Disabled)
				return UniTask.CompletedTask;
			
			if (visualEffectsRepository.TryGet(model.Id, out var effectVisual))
			{
				return resore 
					? effectVisual.ApplyLongEffectAsync() 
					: ChangeEffectAsync(model);
			}
			
			effectVisual = visualEffectsFactory.Create(model);
			if (effectVisual == null)
			{
				DefaultSharedLogger.Error($"{nameof(ApplyLongEffectAsync)}.{nameof(effectVisual)} created as null.");
				return UniTask.CompletedTask;
			}

			visualEffectsRepository.TryAdd(effectVisual);
			return effectVisual.ApplyLongEffectAsync();
		}

		public void InitLongEffect(IRuntimeEffectModel model)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None)
				return;
			
			if (visualEffectsRepository.TryGet(model.Id, out var effectVisual))
			{
				effectVisual.Sync(model);
				return;
			}
			
			effectVisual = visualEffectsFactory.Create(model);
			if (effectVisual == null)
			{
				DefaultSharedLogger.Error($"{nameof(InitLongEffect)}.{nameof(effectVisual)} created as null.");
				return;
			}

			visualEffectsRepository.TryAdd(effectVisual);
		}

		public UniTask ChangeEffectAsync(IRuntimeEffectModel model)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None
			    || !visualEffectsRepository.TryGet(model.Id, out var effectVisual))
				return UniTask.CompletedTask;

			if (effectVisual == null)
			{
				visualEffectsRepository.Remove(model.Id);
				return UniTask.CompletedTask;
			}
			
			effectVisual.Sync(model);
			return effectVisual.ChangeEffectAsync();
		}

		public async UniTask ExpireLongEffectAsync(IRuntimeEffectModel model)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None
			    || !visualEffectsRepository.TryGet(model.Id, out var effectVisual))
				return;

			visualEffectsRepository.Remove(model.Id);
			if (effectVisual == null)
			{
				DefaultSharedLogger.Error($"{nameof(ExpireLongEffectAsync)}.{nameof(effectVisual)} exist but null.");
				return;
			}
			
			effectVisual.Sync(model);
			await effectVisual.ExpireLongEffectAsync();
			effectVisual.Dispose();
		}
	}
}