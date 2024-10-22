using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Commands.Cmd.DebugCmd;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.ExtraCard)]
	public class ExtraCardEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
				GenerateCardsForUser(target.RuntimeData.OwnerUserId);
		}

		private void GenerateCardsForUser(string userId)
		{
			for (var i = 0; i < RuntimeData.CurrentValue; i++)
			{
				LogicContext.CommandController.Execute<AddCardCmd>(userId, new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash, EffectData.Meta), true);
			}
		}
	}
}