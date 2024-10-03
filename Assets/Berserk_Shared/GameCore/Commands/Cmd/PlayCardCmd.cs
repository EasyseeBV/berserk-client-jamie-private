using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.Exceptions;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class PlayCardArgs
	{
		public int? RelativePositionX { get; set; }
	}

	[CmdRequiredType(Executor = typeof(IRuntimeGameCard))]
	public class PlayCardCmd : Command<PlayCardArgs>
	{
		protected override void OnExecute()
		{
			if (Executor is not IRuntimeGameCard cmdCard)
				throw new Exception($"Provide a card to play as an {nameof(Executor)}");

			if (cmdCard.RuntimeData.OwnerUserId != Executor.RuntimeData.OwnerUserId)
				throw new Exception("Cant play other guy's card!");

			if (cmdCard.RuntimeData.Mana > RuntimePlayer.RuntimeData.Mana)
				throw new Exception("Not enough mana to play this card!");

			if (GameContext.Timer.RuntimeData.State == TimerState.Ended)
				throw new Exception("Can't play a card while not playing!");

			if (RuntimePlayer.UserId != GameContext.Timer.RuntimeData.OwnerId)
				throw new Exception("Can't play a card on someone else's turn!");
			
			if (cmdCard.Data.Type == ObjectType.Hero)
				throw new Exception("Can't play a hero!");
			
			if (ArgsModel == default)
				throw new Exception("Missing required args");

			if (cmdCard.IsTableCard() && LogicContext.TargetConditionRepository.IsFullTable(RuntimePlayer.UserId))
				throw new InvalidActionException(InvalidAction.TableIsFull, $"The maximum number of table cards is already on the table!");
			
			SetPlayedCard(cmdCard);
			RuntimePlayer.SpendLava(cmdCard.RuntimeData.Mana);

			var copyOfModel = Model.Clone();
			copyOfModel.TargetObjectsIds.Clear(); // will exclude adding duplicate targets.
			LogicContext.CommandController.Execute<ApprovedPlayCardCmd>(RuntimePlayer.UserId, copyOfModel, true);
		}

		private void SetPlayedCard(IRuntimeGameCard gameCard)
		{
			var timerData = GameContext.Timer.RuntimeData;
			RuntimePlayer.AddPlayedCard(new RuntimePlayedCardData(gameCard.Data.Id, gameCard.RuntimeData.Id, timerData.Round, timerData.Turn), false);
		}

		protected override void OnCancel() // do not use Throw exception in OnCancel method
		{
			if (!Executed)
				return;
			
			if (Executor is not IRuntimeGameCard cmdCard)
			{
				DefaultSharedLogger.Error($"Provide a card to play as an {nameof(Executor)}");
				return;
			}
			
			var lastPlayedCard = RuntimePlayer.RuntimeData.PlayedCards?.LastOrDefault();
			if (lastPlayedCard.HasValue && lastPlayedCard.Value.RuntimeId == cmdCard.RuntimeData.Id)
				RuntimePlayer.RemovePlayerCardAt(RuntimePlayer.RuntimeData.PlayedCards.Count -1, false);
			
			cmdCard.ResetEffects();
			cmdCard.ResetBuffStats();
			RuntimePlayer.RuntimeData.Mana.Add(cmdCard.RuntimeData.Mana); // will trigger callback to send all changes
			cmdCard.ReturnToHand();
		}
	}
}
