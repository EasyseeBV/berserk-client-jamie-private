using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.NextTurn)]
	public class NextTurnNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var turnMem = SessionProcessor.Context.Timer.RuntimeData.Turn;
			var botId = SessionProcessor.Context.PlayerRepository.Get(x => x.RuntimeData.IsBot).UserId;
			
			if (SessionProcessor.Context.Timer.RuntimeData.OwnerId != botId 
			    || turnMem != SessionProcessor.Context.Timer.RuntimeData.Turn)
				return BehaviourNodeState.Failure;

			var model = new CmdParamsModel(SessionProcessor.Context.Timer.RuntimeData.TimeHash);
			SessionProcessor.LogicContext.CommandController.Execute<PassTurnCmd>(botId, model, true);
			return BehaviourNodeState.Success;
		}
	}
}