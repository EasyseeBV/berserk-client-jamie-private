using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class PlayCardDebugCmd : Command
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
				throw new Exception("Card ID not found");

			var cmdCard = LogicContext.RuntimeFactory.CreateRuntimeCard(cardId, ownerId);
			if (cmdCard == null)
				throw new NullReferenceException($"The card could not be created by Id {cardId}");

			SetPlayedCard(cmdCard);
			var model = new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash, new PlayCardArgs(), Model.CommandId)
			{
				ExecutorObjectId = cmdCard.RuntimeData.Id
			};
			LogicContext.CommandController.Execute<ApprovedPlayCardCmd>(RuntimePlayer.UserId, model, true);
		}
		
		private void SetPlayedCard(IRuntimeGameObject gameObject)
		{
			var timerData = GameContext.Timer.RuntimeData;
			RuntimePlayer.AddPlayedCard(new RuntimePlayedCardData(gameObject.Data.Id, gameObject.RuntimeData.Id, timerData.Round, timerData.Turn));
		}

	}
}