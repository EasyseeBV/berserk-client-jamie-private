using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.CustomNodes
{
	[CreateNodeMenu("AI/Custom/Nothing")]
	public class NothingNode : BaseBehaviourNode
	{
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.Nothing;
	}
}