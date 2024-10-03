using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	[CmdRequiredType(Executor = typeof(IRuntimeGameCard))]
	public class SacrificeCardCmd : Command
	{
		protected override void OnExecute()
		{
			if (Executor is not IRuntimeGameCard cmdCard)
				throw new Exception($"Provide a card to play as an {nameof(Executor)}");

			if (cmdCard.RuntimeData.OwnerUserId != Executor.RuntimeData.OwnerUserId)
				throw new Exception("Cant sacrifice other guy's card!");

			if (GameContext.Timer.RuntimeData.State == TimerState.Ended)
				throw new Exception("Can't sacrifice a card while not playing!");

			if (RuntimePlayer.UserId != GameContext.Timer.RuntimeData.OwnerId)
				throw new Exception("Can't sacrifice a card on someone else's turn!");

			RuntimePlayer.RuntimeData.Mana.Add(cmdCard.RuntimeData.Mana);
			cmdCard.ReturnToDiscard();
		}
	}
}
