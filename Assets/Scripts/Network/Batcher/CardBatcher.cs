using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Network;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using Vulcan.Data;
using Vulcan.Network.Context;
using Vulcan.Network.Resolver;
using CardData = Vulcan.Data.CardData;

namespace Vulcan.Network
{
	public class CardBatcher
	{
		public static Batch PrepareBatch(MessageType messageType, Owner batchOwner, int cardsCount,
			params CardData[] data)
		{
			if (batchOwner == Owner.Opponent && !ActorsContextResolver.Opponent.IsControlledByAI)
			{
				RRLogger.Error("Attempt to send someone else's batches");
				return null;
			}

			var sessionPlayer = ActorsContextResolver.GetPlayer(batchOwner);
			var model = new SessionPlayerHandChangeModel
			{
				SessionPlayerId = sessionPlayer.Id,
				HandCardsCount = cardsCount,
				Cards = data.Select(x => x.ToInteractiveCardModel()).ToList()
			};
			return new Batch(model, messageType, sessionPlayer.UserName);
		}

		public static async Task SendRequestAsync(List<Batch> batches)
		{
			if (batches.Count == 0 || BatchController.IsMuted)
				return;

			foreach (var batch in batches)
				RequestsResolver.AddToPending(batch.MessageType);

			// await GameAPI.PlayGameActionAsync(batches);
			RRLogger.Log(
				$"[{"Batcher".Purple().Bold()}] Sent {batches.Count} batches: {string.Join(";", batches.Select(x => x.MessageType))}");
		}
	}
}