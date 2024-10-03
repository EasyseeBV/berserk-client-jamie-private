using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.ControllNodes
{
	[CreateNodeMenu("AI/Base/Selector")]
	public class SelectorNode : BaseBehaviourOutputNode
	{
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.Selector;
	}
}