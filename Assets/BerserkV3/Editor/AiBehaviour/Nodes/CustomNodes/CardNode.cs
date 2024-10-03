using Berserk.Shared.GameCore.AiBehaviour.Models;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;
using UnityEngine;

namespace BerserkV3.GameCore.AiBehaviour.Nodes.CustomNodes
{
	public abstract class CardNode : BaseBehaviourNode
	{
		[SerializeField]
		private CardNodeModel model;

		public override object Model => model;
	}
}