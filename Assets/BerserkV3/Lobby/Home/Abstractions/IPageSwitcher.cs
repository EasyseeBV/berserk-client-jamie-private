using System;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.UIKit;

namespace BerserkV3.Lobby.Home.Abstractions
{
	public interface IPageSwitcher
	{
		int Page { get; }
		event Action<int> OnPageSwitched;
		
		void Init(IStateMachine stateMachine, ISwitcherView view, int? startPage = null);
		void Switch(string stateId, params object[] args);
		void Switch(int page = 0, params object[] args);
		string GetPageName(int page);
		void Release();
	}
}