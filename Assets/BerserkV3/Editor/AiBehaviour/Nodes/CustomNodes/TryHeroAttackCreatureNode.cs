using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.CustomNodes
{
	[CreateNodeMenu("AI/Custom/TryHeroAttackCreature")]
	public class TryHeroAttackCreatureNode : BaseBehaviourNode
	{
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.TryHeroAttackCreature;
	}
}