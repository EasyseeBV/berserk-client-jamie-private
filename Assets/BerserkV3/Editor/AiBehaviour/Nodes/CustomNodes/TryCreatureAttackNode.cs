using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.CustomNodes
{
	[CreateNodeMenu("AI/Custom/TryCreatureAttack")]
	public class TryCreatureAttackNode : CardNode
	{
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.TryCreatureAttack;
	}
}