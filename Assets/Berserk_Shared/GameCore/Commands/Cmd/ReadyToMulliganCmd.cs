using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class ReadyToMulliganCmd : Command
	{
		protected override void OnExecute()
		{
			if (RuntimePlayer != null && GameContext.RuntimeData.MatchMode != MatchMode.Tutorial)
				throw new InvalidOperationException("Can't start mulligan from client in non tutorial mode.");
			
			if (GameContext.Timer.RuntimeData.State >= TimerState.Mulligan)
				throw new InvalidOperationException("Can't start mulligan twice");
			
			GameContext.Timer.SetState(TimerState.Mulligan);
			foreach (var hero in GameContext.GameRuntimePool.GetHeroes())
				LogicContext.EffectExecutor.CreateAppliedEffectAuto(EffectKeyword.Mulligan.AsSystemEffectId(), hero);
		}
	}
}