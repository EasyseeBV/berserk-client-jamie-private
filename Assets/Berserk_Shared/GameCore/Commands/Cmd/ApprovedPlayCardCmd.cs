using System;
using System.Diagnostics;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class ApprovedPlayCardCmd : Command<PlayCardArgs>
	{
		protected override void OnExecute()
		{
			if (Executor is not IRuntimeGameCard cmdCard)
				throw new Exception($"Provide a card to play as an {nameof(Executor)}");

			if (GameContext.Timer.RuntimeData.State == TimerState.Ended)
				throw new Exception("Can't play a card while not playing!");
			
			if (cmdCard.Data.Type == ObjectType.Hero)
				throw new Exception("Can't play a hero!");
			
			try
			{
				if (ArgsModel.RelativePositionX.HasValue)
					cmdCard.RuntimeData.SetRelativePositionX(ArgsModel.RelativePositionX.Value);
				else 
					cmdCard.RuntimeData.ResetRelativePositionX();
				
				cmdCard.ReturnToTable();
				LogicContext.EffectExecutor.CreateStartedEffects(cmdCard);
				cmdCard.Spawn();
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error($"Command was executed correctly but, application throw an error : {e.Message}, " +
				                          $"{GetType().Name} cannot be undone.\nStackTrace : {new StackTrace(e)}");
			}
		}
	}
}