using System;
using Berserk.Shared.Data.Game;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Applications;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	/// <summary>
	///     Visual sequence of VFX / animations etc, corresponding to the visual effect keyword
	/// </summary>
	public abstract class EffectVisual : IDisposable
	{
		protected IVfxApplication VfxApplication { get; private set; }
		protected IGameRepository GameRepository { get; private set; }
		protected IVisualConfigRepository VisualConfigRepository { get; private set; }
		
		public IRuntimeEffectModel Model { get; private set; }
		public IRuntimeObjectView Executor => Model.Executor;
		public IRuntimeObjectView[] Targets => Model.Targets;
		public EffectData EffectData => Model.Data;
		
		public EffectVisual Build(
			IVfxApplication vfxApplication,
			IGameRepository gameRepository,
			IVisualConfigRepository configRepository)
		{
			VfxApplication = vfxApplication;
			GameRepository = gameRepository;
			VisualConfigRepository = configRepository;
			return this;
		}

		public void Sync(IRuntimeEffectModel model)
		{
			Model = model;
		}

		public virtual UniTask PlaySingleEffectAsync()
		{
			return UniTask.CompletedTask;
		}

		public virtual UniTask StartEffectAsync()
		{
			return UniTask.CompletedTask;
		}

		public virtual UniTask EndEffectAsync()
		{
			return UniTask.CompletedTask;
		}

		public virtual UniTask ApplyLongEffectAsync()
		{
			return UniTask.CompletedTask;
		}

		public virtual UniTask ChangeEffectAsync()
		{
			return UniTask.CompletedTask;
		}

		public virtual UniTask ExpireLongEffectAsync()
		{
			return UniTask.CompletedTask;
		}
		
		public virtual void SoftStop()
		{
			Dispose();
		}

		public virtual void Dispose(){}
	}
}