using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.LogicEvents;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public readonly struct RequestPlayersStatisticEvent : ISharedEvent
	{
		public string WinnerId { get; }
		public string LoserId { get; }
		public RequestPlayersStatisticEvent(string winnerId, string loserId)
		{
			WinnerId = winnerId;
			LoserId = loserId;
		}
	}
	
	public class GameEndParams
	{
		public GameEndReason Reason { get; set; }
	}

	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class GameEndCmd : Command<GameEndParams>
	{
		protected override void OnExecute()
		{
			GameContext.SharedEventsSource.Publish(new RequestPlayersStatisticEvent(GetWinner(), GetLooser()));
			var endGameEvent = new EndGame(GetWinner(), GetLooser(), GetReason());
			GameContext.Timer.SetState(TimerState.Ended);
			LogicContext.LogicQueueController.Add(endGameEvent);
			LogicContext.LogicQueueController.SendAndClearLogicQueue(); // force all remaining events to be sent
			
			GameContext.End();
			GameContext.SharedEventsSource.Publish(endGameEvent);
		}

		public virtual string GetLooser()
		{
			return RuntimePlayer.UserId;
		}

		public virtual string GetWinner()
		{
			return GameContext.PlayerRepository.GetOpposite(GetLooser()).UserId;
		}

		public virtual GameEndReason GetReason()
		{
			return ArgsModel.Reason;
		}
	}
}