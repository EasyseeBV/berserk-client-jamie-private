using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.Home.Abstractions;

namespace BerserkV3.Lobby.Home
{
	public class PageSwitcher : IPageSwitcher
	{
		private readonly IGameDatabase gameDatabase;
		private IStateMachine parentStateMachine;
		private ISwitcherView switcherView;
		private List<string> toggleNames;

		public int Page { get; private set; }
		public event Action<int> OnPageSwitched;

		public PageSwitcher(IGameDatabase gameDatabase)
		{
			this.gameDatabase = gameDatabase;
		}

		public void Init(IStateMachine stateMachine, ISwitcherView view, int? startPage = null)
		{
			if (view == null)
				return;

			view.Init();
			parentStateMachine = stateMachine;
			switcherView = view;
			toggleNames = parentStateMachine.GetStates()
				.Select(state => gameDatabase.GetLocalization($"Client_MainMenu_{state.Id}_Title"))
				.ToList();
			
			Page = startPage ?? Page;
			switcherView.AddOrRefresh(toggleNames, Page);
			switcherView.OnToggleSelected += OnPageSwitchRequest;
		}

		public void Switch(string stateId, params object[] args)
		{
			if (switcherView == null)
				return;

			SwitchPage(null, stateId, args);
			switcherView.SwitchWithoutNotify(Page);
		}
		
		public void Switch(int page = 0, params object[] args)
		{
			if (switcherView == null)
				return;

			SwitchPage(page, null, args);
			switcherView.SwitchWithoutNotify(Page);
		}

		public string GetPageName(int page)
		{
			return toggleNames[page];
		}

		public void Release()
		{
			OnPageSwitched = null;
			if (switcherView == null)
				return;
			
			switcherView.Release();
			switcherView.OnToggleSelected -= OnPageSwitchRequest;
			switcherView = null;
		}

		private void OnPageSwitchRequest(int pageIndex)
		{
			if (switcherView == null)
				return;
			
			SwitchPage(pageIndex, null);
		}

		private void SwitchPage(int? index, string id, params object[] args)
		{
			if (string.IsNullOrEmpty(id) && !index.HasValue)
				throw new ArgumentException("Provide index or state id to switch to page.");

			var state = parentStateMachine.GetState(id) 
			            ?? parentStateMachine.GetState(index!.Value);
			
			Page = state.Order;
			parentStateMachine.Switch(state.Id, args);
			OnPageSwitched?.Invoke(Page);
		}
	}
}