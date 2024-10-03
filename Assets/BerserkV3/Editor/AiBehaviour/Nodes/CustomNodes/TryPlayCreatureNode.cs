using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.CustomNodes
{
	[CreateNodeMenu("AI/Custom/TryPlayCreature")]
	public class TryPlayCreatureNode : CardNode
	{
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.TryPlayCreature;
	}
}