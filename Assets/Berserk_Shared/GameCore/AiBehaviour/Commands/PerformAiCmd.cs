using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.Commands;

namespace Berserk.Shared.GameCore.AiBehaviour.Commands
{
	public readonly struct AiCommandExecuteEvent : ISharedEvent
	{
		public RuntimeAiArg CommandArg { get; }

		public AiCommandExecuteEvent(RuntimeAiArg commandArg)
		{
			CommandArg = commandArg;
		}
	}

	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class PerformAiCmd : Command<RuntimeAiArg>
	{
		protected override void OnExecute()
		{
			if (GameContext.RuntimeData.MatchMode != MatchMode.Tutorial)
				throw new InvalidOperationException("Can't ai command in non Tutorial matches.");

			if (GameContext.PlayerRepository.Get(x=> x.RuntimeData.IsBot)?.RuntimeData is not RuntimeAiData runtimeData)
				throw new ArgumentException("Ai must have his own runtime data.");

			runtimeData.CommandBuffer.Add(ArgsModel);
			GameContext.SharedEventsSource.Publish(new AiCommandExecuteEvent(ArgsModel));
		}
	}
}