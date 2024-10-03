using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.AiBehaviour.Models
{
	[Serializable]
	public class AiNode
	{
		public AiBehaviourNodeType NodeType;
		public object Model;
		public List<AiNode> Childs;
	}

	public class NodeModel
	{
		public string Type;
		public string CardId;
		public string TargetCardId;
	}
}