using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.CheckTurn)]
	public class CheckTurnNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			if (node.Model is IntConditionNodeModel model && model.Check(SessionProcessor.Context.Timer.RuntimeData.Turn))
				return BehaviourNodeState.Success;

			return BehaviourNodeState.Failure;
		}
	}
}