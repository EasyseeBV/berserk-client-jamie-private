using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;

namespace Berserk.Shared.GameCore.Commands.Cmd.DebugCmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class TimerCmd : Command<string[]>
	{
		protected override void OnExecute()
		{
			var timer = GameContext.Timer;
			foreach (var arg in ArgsModel)
			{
				if(string.IsNullOrEmpty(arg))
					continue;
				
				switch (arg.ToLower())
				{
					case "pause":
					case "stop":
					case "break":
						timer.Pause();
						return;
				
					case "resume" :
					case "play" :
					case "unpause":
						timer.Unpause();
						return;
				}
			}
		}
	}
}