using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.Inverter)]
	public class InverterNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var resultState = BehaviourNodeState.Success;
			if (node.Childs != null)
				foreach (var outputNode in node.Childs)
				{
					switch (behaviourExecutor.ExecuteNode(outputNode))
					{
						case BehaviourNodeState.Running:
							resultState = BehaviourNodeState.Running;
							break;
						case BehaviourNodeState.Success:
							resultState = BehaviourNodeState.Failure;
							break;
						case BehaviourNodeState.Failure:
							resultState = BehaviourNodeState.Success;
							break;
					}
				}

			return resultState;
		}
	}
}