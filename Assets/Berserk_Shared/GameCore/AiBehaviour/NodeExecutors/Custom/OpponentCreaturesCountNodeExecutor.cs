using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.OpponentCreaturesCount)]
	public class OpponentCreaturesCountNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			var opponentState = SessionProcessor.Context.PlayerRepository.Get(x => !x.RuntimeData.IsBot);
			var creaturesCount = SessionProcessor.Context.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, opponentState.UserId, ObjectType.Creature)
				.Count();

			if (node.Model is IntConditionNodeModel model && model.Check(creaturesCount))
				return BehaviourNodeState.Success;

			return BehaviourNodeState.Failure;
		}
	}
}