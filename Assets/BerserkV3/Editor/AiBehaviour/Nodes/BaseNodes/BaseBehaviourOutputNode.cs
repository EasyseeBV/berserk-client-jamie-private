using System.Collections.Generic;
using System.Linq;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes
{
	public abstract class BaseBehaviourOutputNode : BaseBehaviourNode
	{
		[Output] public Empty Nodes;

		public IEnumerable<BaseBehaviourNode> GetOutputNodes()
		{
			var output = GetOutputPort("Nodes");
			if (output != null && output.ConnectionCount > 0)
				return output.GetConnections()
					.Select(n => n.node as BaseBehaviourNode)
					.OrderBy(n => n != null ? n.position.x : 0);
			return null;
		}
	}
}