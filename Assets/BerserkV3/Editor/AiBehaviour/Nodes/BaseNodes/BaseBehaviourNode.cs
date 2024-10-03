using Berserk.Shared.Data.Enums;
using XNode;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes
{
	public abstract class BaseBehaviourNode : Node
	{
		[Input] public Empty Enter;
		public abstract AiBehaviourNodeType NodeType { get; }
		public virtual object Model => null;
	}
}