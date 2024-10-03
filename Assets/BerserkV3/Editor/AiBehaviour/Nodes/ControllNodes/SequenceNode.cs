using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.ControllNodes
{
	[CreateNodeMenu("AI/Base/Sequence")]
	public class SequenceNode : BaseBehaviourOutputNode
	{
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.Sequence;
	}
}