using System;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public interface IAutoMatchSearchApplication
	{
		event Action OnCancel;
		void Searching();
		void Accepting();
		void Leaving();
		void Stop();
	}
}