using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Commands.Cmd.DebugCmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class MannaCmd : Command<string[]>
	{
		protected override void OnExecute()
		{
			var userId = ArgsModel
				.SelectWhere(arg => (TryGetUserId(arg, out var userId), userId))
				.FirstOrDefault() ?? RuntimePlayer.UserId;
			
			foreach (var command in ArgsModel)
				ExecuteCommand(command, userId);
		}

		private void ExecuteCommand(string command, string userId)
		{
			switch (command.ToLower().Trim())
			{
				case "disable":
				case "pause":
				case "stop":
				{
					var stat = GameContext.PlayerRepository.Get(userId).RuntimeData.Mana;
					var modifier = stat.GetModifiers(nameof(MannaCmd)).FirstOrDefault();
					if (modifier != null)
						return;
					modifier = new ClampedPercentModifierInt()
						.SetPercentSource(PercentModifierSource.Maximum)
						.SetModifierId(nameof(MannaCmd))
						.SetMaxModifier(-100)
						.SetCurrModifier(-100)
						.SetPriority((int) EffectExecutionOrder.Last);

					stat.AddModifier(modifier);
					return;
				}
				
				case "enable" :
				case "unpause" :
				case "play" :
				{
					var stat = GameContext.PlayerRepository.Get(userId).RuntimeData.Mana;
					stat.RemoveModifiers(nameof(MannaCmd));
					return;
				}
			}
		}

		private bool TryGetUserId(string arg, out string userId)
		{
			switch(arg.ToLower().Trim())
			{
				case "o" :
					userId = GameContext.PlayerRepository.GetOpposite(RuntimePlayer.UserId).UserId;
					return true;
					
				case "s" :
					userId = RuntimePlayer.UserId;
					return true;
					
				default:
					userId = null;
					return false;
			};
		}
	}
}