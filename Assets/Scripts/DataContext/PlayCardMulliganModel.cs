using System;

namespace ServerCore.Infrastructure.Models
{
	[Obsolete("Client not used this model")]
	public class PlayCardMulliganModel : SessionGameModel
	{
		public string[] SessionCardIds { get; set; }

		public override string ToString()
		{
			var message =
				$"SessionPlayerId - {SessionPlayerId}\n" +
				$"{SessionCardIds.Length} cards: [{string.Join(" ; ", SessionCardIds)}]";

			return message;
		}
	}
}