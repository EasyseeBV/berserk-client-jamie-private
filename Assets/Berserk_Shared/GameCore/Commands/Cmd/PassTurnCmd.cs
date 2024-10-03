using System;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class PassTurnCmd : Command
	{
		protected override void OnExecute()
		{
			if (GameContext.Timer.RuntimeData == null)
				throw new Exception($"Timer not found for {RuntimePlayer.UserId}");

			if (RuntimePlayer.UserId != GameContext.Timer.RuntimeData.OwnerId)
				throw new Exception("It's not your turn");

			if (GameContext.Timer.RuntimeData.State < TimerState.Game)
				throw new Exception("You can't change the turn, the game hasn't started.");

			NextTurn();
		}

		protected virtual void NextTurn()
		{
			if (GameContext.Timer.RuntimeData.State <= TimerState.Ready)
			{
				GameContext.Timer.SetState(TimerState.Game, false);
				
				if (GameContext.RuntimeData.MatchMode == MatchMode.Tutorial)
					GameContext.Timer.Pause(false);
			}
			
			GameContext.Timer.NextTurn();
		}
	}
}