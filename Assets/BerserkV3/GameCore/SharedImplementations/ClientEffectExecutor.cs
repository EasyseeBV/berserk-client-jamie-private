using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem;

namespace BerserkV3.GameCore.SharedImplementations
{
	public class ClientEffectExecutor : IEffectExecutor
	{

		public IEnumerable<IRuntimeEffect> CreateStartedEffects(IRuntimeGameObject executor)
		{
			return Array.Empty<IRuntimeEffect>();
		}

		public IRuntimeEffect CreateAppliedEffectAuto(
			string effectConfigId,
			IRuntimeGameObject effectOwner,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] manualTargets)
		{
			return default;
		}

		public IRuntimeEffect CreateAndExecuteEffect(
			string effectConfigId,
			IRuntimeGameObject effectOwner,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			return default;
		}

		public IRuntimeEffect CreateAndExecuteEffectAuto(
			string effectConfigId, 
			IRuntimeGameObject effectOwner,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeGameObject[] targets)
		{
			return default;
		}

		public bool ExecuteEffect(IRuntimeEffect effectRuntime)
		{
			return true;
		}

		public bool ExecuteAppliedEffect(IRuntimeEffect effect)
		{
			return true;
		}

		public void ExecutePhase(PhaseInfo info) {}
		public void ExpireAppliedEffects(IRuntimeGameObject executor, ExpirePhase expirePhase) {}
		public void ResetBuffEffectsAndStats(IRuntimeGameObject card) {}
		public void Dispose() {}
	}
}