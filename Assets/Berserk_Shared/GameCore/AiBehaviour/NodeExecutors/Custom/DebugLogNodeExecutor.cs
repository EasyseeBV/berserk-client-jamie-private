using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.DebugLog)]
	public class DebugLogNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node,IAiBehaviourExecutor behaviourExecutor)
		{
			DefaultSharedLogger.Log((node.Model as DebugLogNodeModel)?.Message);
			return BehaviourNodeState.Success;
		}
	}
}