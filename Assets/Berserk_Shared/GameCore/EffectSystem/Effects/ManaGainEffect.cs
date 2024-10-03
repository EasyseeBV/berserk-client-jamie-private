using System.Linq;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.ManaGain)]
	public class ManaGainEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			var players = GetExecutionTargets()
				.Select(t => GameContext.PlayerRepository.Get(t.RuntimeData.OwnerUserId))
				.Distinct()
				.ToArray();
			
			foreach (var player in players)
			{
				var value = ValueModRounded(player.RuntimeData.Mana);
				player.RuntimeData.Mana.SetOrRaiseMax(player.RuntimeData.Mana + value);
			}
		}
	}
}