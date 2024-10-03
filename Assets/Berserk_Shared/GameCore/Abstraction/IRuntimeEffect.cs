using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeEffect : IDisposable
	{
		IRuntimeGameObject[] Targets { get; }
		IRuntimeGameObject Executor { get; }
		EffectData EffectData { get; }
		IRuntimeEffectData RuntimeData { get; }

		IRuntimeEffect Sync(IRuntimeEffectData runtimeData);
		void Create();
		void Expire();
		void Execute();
		bool CanExecute();
		void SetTargets(IRuntimeGameObject[] targets);
		void Disable(int length);

		/// <summary>
		/// Callback after lifetime was changed it means the expire phase passed.
		/// </summary>
		void OnExpirePhase();
		/// <summary>
		/// Callback after all the main logics executed.
		/// </summary>
		void OnExecuted();
		/// <summary>
		/// Callback after effect deleted from entity, only when effect was applied.
		/// </summary>
		void OnDeleted();
		/// <summary>
		/// Callback after applied effect has changed some where outside.
		/// </summary>
		void OnChanged();
		/// <summary>
		/// Callback after applied effect was add to entity as applied.
		/// </summary>
		void OnAdded();
		IRuntimeEffect Stack(IRuntimeEffect other);
		IRuntimeGameObject[] GetExecutionTargets();
	}
}