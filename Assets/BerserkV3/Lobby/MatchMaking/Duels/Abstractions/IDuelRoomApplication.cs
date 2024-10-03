using System;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public interface IDuelRoomApplication
	{
		UniTask OpenAsync(DuelRoomItemData data, Action onReturn = null);
	}
}