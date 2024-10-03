using System;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Applications;
using BerserkV3.GameCore.EffectsVisual.Visuals;
using BerserkV3.GameCore.Repository;
using RR.Core.DebugSystem;
using Zenject;

namespace BerserkV3.GameCore.EffectsVisual.Factories
{
	public class VisualEffectsFactory : IVisualEffectsFactory
	{
		private readonly IInstantiator instantiator;
		private readonly IVfxApplication vfxApplication;
		private readonly IGameRepository gameRepository;
		private readonly IVisualEffectTypeCollection typeCollection;
		private readonly IVisualConfigRepository visualConfigRepository;

		public VisualEffectsFactory(
			IInstantiator instantiator,
			IVfxApplication vfxApplication,
			IGameRepository gameRepository,
			IVisualEffectTypeCollection typeCollection,
			IVisualConfigRepository visualConfigRepository)
		{
			this.instantiator = instantiator;
			this.vfxApplication = vfxApplication;
			this.gameRepository = gameRepository;
			this.typeCollection = typeCollection;
			this.visualConfigRepository = visualConfigRepository;
		}

		public EffectVisual Create(IRuntimeEffectModel model)
		{
			if (model.Data.VisualKeyword == EffectVisualKeyword.None)
				return null;

			if (model.Executor == null)
			{
				RRLogger.Error($"{nameof(model.Executor)} view not found, {nameof(EffectVisualKeyword)} : {model.Data.VisualKeyword}");
				return null;
			}

			if (!typeCollection.TryGetType(model.Data.VisualKeyword, out var effectType))
				throw new Exception($"Can't find type for {model.Data.VisualKeyword}");

			var obj = instantiator.Instantiate(effectType);
			if (obj is not EffectVisual effectVisual)
				throw new InvalidCastException($"{obj} - cannot casted to {nameof(EffectVisual)}");
			
			effectVisual.Build(vfxApplication, gameRepository, visualConfigRepository).Sync(model);
			return effectVisual;
		}
	}
}