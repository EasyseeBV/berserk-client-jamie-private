using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;

namespace Berserk.Shared.GameCore.Commands.Cmd.DebugCmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class AddManaCmd : Command<int>
	{
		protected override void OnExecute()
		{
			RuntimePlayer.RuntimeData.Mana.SetOrRaiseMax(RuntimePlayer.RuntimeData.Mana + ArgsModel);
		}
	}
}