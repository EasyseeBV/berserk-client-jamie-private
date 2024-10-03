using System;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.AiBehaviour.Attributes
{
	public class AiBehaviourExecutorAttribute : Attribute
	{
		public AiBehaviourNodeType NodeType;

		public AiBehaviourExecutorAttribute(AiBehaviourNodeType nodeType)
		{
			NodeType = nodeType;
		}
	}
}
