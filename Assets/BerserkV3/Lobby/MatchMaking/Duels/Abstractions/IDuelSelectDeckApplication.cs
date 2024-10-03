using System;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public interface IDuelSelectDeckApplication
	{
		UniTask OpenAsync(Action onReturn = null);
		UniTask CloseAsync();
		UniTask Select(string deckId);
		bool Validate(string deckId);
	}
}