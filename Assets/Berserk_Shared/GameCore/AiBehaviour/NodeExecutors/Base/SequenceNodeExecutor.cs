using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.Sequence)]
	public class SequenceNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var isAnyNodeRunning = false;
			if (node.Childs != null)
			{
				foreach (var outputNode in node.Childs)
				{
					switch (behaviourExecutor.ExecuteNode(outputNode))
					{
						case BehaviourNodeState.Running:
							isAnyNodeRunning = true;
							break;
						case BehaviourNodeState.Success:
							break;
						case BehaviourNodeState.Failure:
							return BehaviourNodeState.Failure;
					}
				}

				return isAnyNodeRunning ? BehaviourNodeState.Running : BehaviourNodeState.Success;
			}

			return BehaviourNodeState.Failure;
		}
	}
}