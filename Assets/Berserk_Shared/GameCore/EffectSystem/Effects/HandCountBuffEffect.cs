using System.Linq;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.HandCountBuff)]
	public class HandCountBuffEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			var players = GetExecutionTargets()
				.Select(t => GameContext.PlayerRepository.Get(t.RuntimeData.OwnerUserId))
				.Distinct()
				.ToArray();
			
			foreach (var player in players)
			{
				var value = ValueModRounded(player.RuntimeData.HandCount);
				player.RuntimeData.HandCount.SetOrRaiseMax(player.RuntimeData.HandCount + value);
			}
		}

		protected override void OnExpire()
		{
			base.OnExpire();
			var players = GetExecutionTargets()
				.Select(t => GameContext.PlayerRepository.Get(t.RuntimeData.OwnerUserId))
				.Distinct()
				.ToArray();
			
			foreach (var player in players)
			{
				var value = ValueModRounded(player.RuntimeData.HandCount);
				player.RuntimeData.HandCount.SetOrRaiseMax(player.RuntimeData.HandCount - value);
			}
		}
	}
}