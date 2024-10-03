using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;
using UnityEngine;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.CustomNodes
{
	[CreateNodeMenu("AI/Custom/CheckTurn")]
	public class CheckTurnNode : BaseBehaviourNode
	{
		[SerializeField]
		private IntConditionNodeModel model;

		public override object Model => model;
		public override AiBehaviourNodeType NodeType => AiBehaviourNodeType.CheckTurn;
	}
}