using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;

namespace Berserk.Shared.GameCore.Commands.Cmd.DebugCmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class AddCardCmd : Command
	{
		protected override void OnExecute()
		{
			if(string.IsNullOrEmpty(Meta))
				return;
			
			var cardId = new string(Meta.Where(char.IsDigit).ToArray());
			var cardData = GameContext.GameDatabase.GetCard(cardId);
			var ownerId = Meta.Contains("o")
				? GameContext.PlayerRepository.GetOpposite(RuntimePlayer.UserId).UserId
				: RuntimePlayer.UserId;
			if (cardData == null)
				throw new Exception("Set card ID not found");

			var runtimeCard = LogicContext.RuntimeFactory.CreateRuntimeCard(cardId, ownerId);
			if (runtimeCard == null)
				throw new NullReferenceException($"The card could not be created by Id {cardId}");
			
			runtimeCard.ReturnToHand();
			runtimeCard.ChangeEffectPhase(EffectPhase.AfterDraw);
		}
	}
}