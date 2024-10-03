using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class TutorialEnd : GameEndCmd
	{
		protected override void OnExecute()
		{
			if (GameContext.RuntimeData.MatchMode != MatchMode.Tutorial)
				throw new InvalidOperationException($"Cannot end game in current match mode :{GameContext.RuntimeData.MatchMode}");
			
			base.OnExecute();
		}

		public override string GetLooser()
		{
			return GameContext.PlayerRepository.GetOpposite(RuntimePlayer.UserId).UserId;
		}

		public override string GetWinner()
		{
			return RuntimePlayer.UserId;
		}
	}

}