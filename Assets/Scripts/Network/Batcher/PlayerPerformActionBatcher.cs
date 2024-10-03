using System.Linq;
using Berserk.Shared.Data.Enums;
using Game.Entities;
using ServerCore.Infrastructure.Models;
using Vulcan.Data;
using Vulcan.Network.Context;
using EffectPhase = Vulcan.Data.EffectPhase;

namespace Vulcan.Network
{
	public sealed class PlayerPerformActionBatcher
	{
		public static Batch PrepareBatch(IInteractiveEntity source,
			IInteractiveEntity[] mutableEntity,
			ActionType attackType,
			EffectPhase phase = EffectPhase.None,
			EffectKeyword effect = EffectKeyword.None,
			bool defenceDamage = true)
		{
			var model = new PerformActionModel
			{
				SessionPlayerId = ActorsContextResolver.Self.Id,
				ActionType = attackType,
				Phase = phase,
				Effect = effect,
				Source = source.ToInteractiveCardModel(),
				Targets = mutableEntity.Select(x => x.ToInteractiveCardModel()).ToList(),
				DefenceDamage = defenceDamage
			};

			return new Batch(model, MessageType.PerformAction, ActorsContextResolver.Self.UserName);
		}
	}
}