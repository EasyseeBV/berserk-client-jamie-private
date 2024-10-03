using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class AcceptOverchargeCmd : Command<OverchargeRuntimeArg>
	{
		protected override void OnExecute()
		{
			if(ArgsModel == null)
				return;
			
			if (RuntimePlayer.UserId != GameContext.Timer.RuntimeData.OwnerId)
				throw new Exception("It's not your turn");
			
			if (GameContext.Timer.RuntimeData.State == TimerState.Ended)
				throw new Exception("Timer state is not Game");
			
			if (GameContext.GameRuntimePool.GetHeroByUserId(RuntimePlayer.UserId).IsDead)
				throw new Exception("Executor is dead");
			
			if(ArgsModel.Count <= 0)
				throw new Exception("Cant play effect!");

			if (!Executor.AppliedEffects.TryGet(x => x.RuntimeData.Id == ArgsModel.RuntimeEffectId, out var effect))
				throw new Exception($"Failed to find an effect {ArgsModel.RuntimeEffectId}");

			var arg = effect.RuntimeData.GetRuntimeArg<OverchargeRuntimeArg>();
			arg.Count = ArgsModel.Count;
			LogicContext.EffectExecutor.ExecuteEffect(effect);
		}
	}
}