using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.Selector)]
	public class SelectorNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			if (node.Childs == null) 
				return BehaviourNodeState.Failure;
			
			foreach (var outputNode in node.Childs)
			{
				var state = behaviourExecutor.ExecuteNode(outputNode);
				if (state is BehaviourNodeState.Running or BehaviourNodeState.Success)
					return state;
			}

			return BehaviourNodeState.Failure;
		}
	}
}