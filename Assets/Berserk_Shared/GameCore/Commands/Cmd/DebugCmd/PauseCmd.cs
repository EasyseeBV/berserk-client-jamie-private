using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;

namespace Berserk.Shared.GameCore.Commands.Cmd.DebugCmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class PauseCmd : Command
	{
		protected override void OnExecute()
		{
			if (GameContext.Timer.RuntimeData == null)
				return;
			
			GameContext.Timer.Pause();
		}
	}
}