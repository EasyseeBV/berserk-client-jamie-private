using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;
using UnityEngine;
using XNode;

namespace BerserkV3.GameCore.AiBehaviour.Scriptables
{
	[CreateAssetMenu(fileName = "BehaviourTreeGraph", menuName = "XBehaviourTree/BehaviourTreeGraph")]
	public class BehaviourGraph : NodeGraph
	{
		[SerializeField] private BaseBehaviourNode enterNode;

		public BaseBehaviourNode EnterNode => enterNode;
	}
}