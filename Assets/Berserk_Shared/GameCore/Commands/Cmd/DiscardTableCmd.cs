using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	[CmdExecutionLevel(CmdExecutionLevel.WrapAllowed)]
	public class DiscardTableCmd : Command
	{
		protected override void OnExecute()
		{
			var args = Meta?.ToLower();
			var players = GameContext.PlayerRepository.Where(player =>
			{
				if (string.IsNullOrEmpty(args))
					return true;

				return player.UserId == RuntimePlayer.UserId && args.Contains("s")
				       || player.UserId != RuntimePlayer.UserId && args.Contains("o");
			});
			
			foreach (var player in players)
			{
				var effectOwner = GameContext.GameRuntimePool.GetHeroByUserId(player.UserId);
				var effectId = EffectKeyword.Discard.AsRuntimeEffectId();
				var targets = GameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InTable, player.UserId, asQuery:true)
					.ToArray<IRuntimeGameObject>();

				LogicContext.EffectExecutor.CreateAndExecuteEffect(effectId, effectOwner, null, targets);
			}
		}
	}
}