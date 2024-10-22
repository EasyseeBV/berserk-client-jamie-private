using System;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public interface IAutoMatchApplication
	{
		AutoMatchStateModel State { get; }
		event Action OnStateChanged;
		
		UniTask JoinAutoMatchAsync(AutoMatchJoinModel joinModel);
		void LeaveAutoMatch();
	}
}