using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Draw)]
	[EffectKeyword(EffectKeyword.Recon)]
	public class DrawEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var ownerId in GetOwnerIds())
			{
				Draw(ownerId, GetCount());
			}
		}

		protected virtual int GetCount()
		{
			return ValueModRounded();
		}

		protected virtual void Draw(string ownerId, int count)
		{
			if (string.IsNullOrEmpty(ownerId))
				throw new InvalidOperationException("UserOwnerId is missing");
			
			if (count < 0)
				throw new InvalidOperationException("Count to draw must be more or equal to zero");
			
			LogicContext.GiveCardsService.RequestGiveCards(ownerId, count);
		}

		protected virtual IEnumerable<string> GetOwnerIds()
		{
			return GetExecutionTargets().Select(x => x.RuntimeData.OwnerUserId).Distinct().ToArray();
		}
	}
}