using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	[CmdExecutionLevel(CmdExecutionLevel.BeforeSessionStared)]
	public class ReadyToPlayCmd : Command
	{
		protected override void OnExecute()
		{
			if (RuntimePlayer.RuntimeData.IsReady)
			{
				DefaultSharedLogger.Log($"[{GetType().Name}] UserName: {RuntimePlayer.RuntimeData.UserName}, I'm ready now.");
				return;
			}
			
			RuntimePlayer.SetReady(true);
			
			if (GameContext.PlayerRepository.All(x => x.RuntimeData.IsReady || x.RuntimeData.IsBot))
				SessionStart();
		}

		private void SessionStart()
		{
			if (GameContext.RuntimeData.IsStarted)
				throw new InvalidOperationException($"Game already started, can't start twice.");
			
			if (!GameContext.GameRuntimePool.Any())
				throw new ArgumentNullException($"{nameof(GameContext.GameRuntimePool)} must be filled with objects before start!");
			
			GameContext.Start();
			
			if (GameContext.RuntimeData.MatchMode != MatchMode.Tutorial)
				LogicContext.CommandController.Execute<ReadyToMulliganCmd>(null, new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash), true);
		}
	}
}