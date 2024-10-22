using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;

namespace Berserk.Shared.GameCore.Commands.Cmd.DebugCmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class FullHpCmd : Command
	{
		protected override void OnExecute()
		{
			var hero = GameContext.GameRuntimePool.GetHeroByUserId(RuntimePlayer.UserId);
			hero.TakeRestore(hero.RuntimeData.Hp.BaseStat, hero);
		}
	}
}