using System;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public interface IAutoMatchAcceptApplication
	{
		event Action<bool> OnAccepted;
		event Action OnTimeout;

		void Close();
		void Found(int countdown);
		void Accepted(bool value);
		void Starting();
		void Timeout();
		void Return(Action onCompleted);
	}
}