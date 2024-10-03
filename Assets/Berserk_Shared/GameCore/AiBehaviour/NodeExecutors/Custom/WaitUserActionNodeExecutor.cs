using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	[AiBehaviourExecutor(AiBehaviourNodeType.WaitUserAction)]
	public class WaitUserActionNodeExecutor : AbstractNodeExecutor
	{
		public override BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor)
		{
			if (node.Model is not StringConditionNodeModel model 
			    || !SessionProcessor.Context.PlayerRepository.TryGet(x => x.RuntimeData.IsBot, out var runtimePlayer)
			    || runtimePlayer.RuntimeData is not RuntimeAiData runtimeData
			    || !runtimeData.CommandBuffer.TryGet(data => !data.Expired, out var commandArg)
			    || !model.Check(commandArg.Id))
				return BehaviourNodeState.Failure;
			
			commandArg.Cycle--;
			commandArg.Expired = commandArg.Cycle == 0;
			return BehaviourNodeState.Success;
		}
	}
}