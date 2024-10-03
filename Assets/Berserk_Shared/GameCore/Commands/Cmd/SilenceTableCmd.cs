using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Commands.Cmd
{
	public class SilenceTableCmd : Command
	{
		protected override void OnExecute()
		{ 
			var userId = Meta.ToLower() switch
			{
				"o" => GameContext.PlayerRepository.GetOpposite(RuntimePlayer.UserId).UserId,
				"s" => RuntimePlayer.UserId,
				_ => null,
			};

			var targets = GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, userId)
				.Cast<IRuntimeGameObject>()
				.ToArray();
			
			var silenceEffectId = EffectKeyword.Silence.AsSystemEffectId();
			var giveEffectId = EffectKeyword.GiveEffects.AsRuntimeEffectId();
			var args = GetEffectArgs(targets, silenceEffectId);
			var effectOwner = GameContext.GameRuntimePool.GetHeroByUserId(userId ?? RuntimePlayer.UserId);
			LogicContext.EffectExecutor.CreateAndExecuteEffect(giveEffectId, effectOwner, args, targets);
		}

		protected virtual IEffectRuntimeArg[] GetEffectArgs(IEnumerable<IRuntimeGameObject> targets, string effectId)
		{
			return targets
				.Select(x => new EffectRuntimeTargetArg(x.RuntimeData.Id))
				.Append<IEffectRuntimeArg>(new GiveEffectIdArg(effectId))
				.ToArray();
		}

	}
}