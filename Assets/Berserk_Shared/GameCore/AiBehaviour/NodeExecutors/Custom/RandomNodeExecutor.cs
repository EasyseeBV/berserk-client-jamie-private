using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.Random)]
	public class RandomNodeExecutor : AbstractNodeExecutor
	{
		private readonly Random random = new();

		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			if (random.Next(0, 100) <= ((IntValueNodeModel)node.Model).Value)
				return BehaviourNodeState.Success;

			return BehaviourNodeState.Failure;
		}
	}
}