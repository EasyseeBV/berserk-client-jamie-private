using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.CustomNodes
{
	[CreateNodeMenu("AI/Custom/TryPlayNegativeCard")]
	public class TryPlayNegativeCardNode : BaseBehaviourNode
	{
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.TryPlayNegativeCard;
	}
}