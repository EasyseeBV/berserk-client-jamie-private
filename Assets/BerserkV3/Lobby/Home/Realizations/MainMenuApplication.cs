using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Utils;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Common.StateMachine;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.Home.Args;
using BerserkV3.Lobby.Home.States;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Abstractions;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Controllers;
using BerserkV3.Lobby.UI.Home.General;
using BerserkV3.Lobby.UI.Home.General.Widgets;
using RR.UIService;
using Zenject;

namespace BerserkV3.Lobby.Home
{
	public class MainMenuApplication : IMainMenuApplication
	{
		private readonly IPageSwitcher pageSwitcher;
		private readonly IStateMachine stateMachine;
		private readonly IInstantiator instantiator;
		private readonly IGameDatabase gameDatabase;
		private readonly ILobbySingalProcessor lobbySingalProcessor;
		private readonly ILobbySettingsApplication settingsApplication;
		private readonly IUIService uiService;
		private readonly ICurrencyPanelPresenter currencyPanelPresenter;
		private readonly List<IState> allLobbyState;
		private readonly int profilePageIndex;
		private readonly int storePageIndex;
		private int onlineCount = 1;
		
		
		public MainMenuApplication(
			IUIService uiService,
			IPageSwitcher pageSwitcher,
			IStateMachine stateMachine,
			IInstantiator instantiator,
			IGameDatabase gameDatabase,
			ILobbySingalProcessor lobbySingalProcessor,
			ILobbySettingsApplication settingsApplication, 
			ICurrencyPanelPresenter currencyPanelPresenter)
		{
			this.uiService = uiService;
			this.pageSwitcher = pageSwitcher;
			this.stateMachine = stateMachine;
			this.instantiator = instantiator;
			this.gameDatabase = gameDatabase;
			this.lobbySingalProcessor = lobbySingalProcessor;
			this.settingsApplication = settingsApplication;
			this.currencyPanelPresenter = currencyPanelPresenter;

			stateMachine.SetId(nameof(MainMenuApplication));
			
			RegisterState<GeneralHomeState>();
			RegisterState<GeneralGameModesState>();
			RegisterState<GeneralStoreState>();
			RegisterState<GeneralCollectionState>();
			RegisterState<GeneralProfileState>();

			var currentStetes = stateMachine.GetStates().ToList();
			profilePageIndex = currentStetes.FindIndex(x => x is GeneralProfileState);
			storePageIndex = currentStetes.FindIndex(x => x is GeneralStoreState);
			allLobbyState = currentStetes
				.SelectMany(x => x.GetAllStates())
				.ToList();
		}
		
		public void Init()
		{
			lobbySingalProcessor.OnMessageReceived += OnLobbyMessageReceived;
			settingsApplication.Init();
			uiService.Begin<GeneralWindow>()
				.WithInit(InitWindowAsync)
				.Show();
			
			pageSwitcher.Switch(pageSwitcher.Page);
			return;
			async void InitWindowAsync(GeneralWindow window)
			{
				currencyPanelPresenter.Init();
				currencyPanelPresenter.OnWalletClicked += () =>
				{
					if (pageSwitcher.Page != storePageIndex)
					{
						pageSwitcher.Switch(storePageIndex);
					}
				};
				currencyPanelPresenter.Show();
				
				pageSwitcher.Init(stateMachine, window.SwitcherView);
				pageSwitcher.OnPageSwitched += PageSwitched;
					
				window.ProfileWidget.SetActive(pageSwitcher.Page != profilePageIndex);
				window.ProfileWidget.SetClickAction(() => pageSwitcher.Switch(profilePageIndex));
				window.SearchingWidget.SetActive(false);
				UpdateOnline(window.OnlineWidget, onlineCount);
				await window.ProfileWidget.InitAsync(null, null, null, "brenopereira", "MMR: 1192"); // TODO fill
				
				return;
				void PageSwitched(int pageIndex)
				{
					window.ProfileWidget.SetActive(pageSwitcher.Page != profilePageIndex);
				}
			}
		}

		private void OnLobbyMessageReceived(SignalType signalType, object obj)
		{
			if (signalType != SignalType.Information
			    || obj is not OnlinePlayersModel onlineModel)
				return;

			onlineCount = onlineModel.Count;
			UpdateOnline(uiService.Get<GeneralWindow>().OnlineWidget, onlineCount);
		}

		private void UpdateOnline(OnlineWidget widget, int count)
		{
			var onlineFormat = gameDatabase.GetLocalization("Client_Lobby_OnlineFormat");
			widget.SetActive(true);
			widget.SetText(gameDatabase.GetLocalization("Client_Lobby_OnlineTitle"));
			widget.SetCountText(string.Format(onlineFormat, count));
		}

		private void ProcessRedirections(params object[] args)
		{
			var redirectId = GetRedirectId(args);
			if (string.IsNullOrEmpty(redirectId) || !allLobbyState.TryGet(x => x.Id == redirectId, out var state)) 
				return;
			
			var path = new List<object>(args);
			//Creating path to the finish state
			while (state.ParentState != null)
			{
				path.Add(new MainMenuSubStateRedirectionArg(state.Id));
				state = state.ParentState;
			}

			pageSwitcher.Switch(state.Id, path.ToArray());
		}

		public void Redirect(params object[] args)
		{
			ProcessRedirections(args);
		}
		
		private static string GetRedirectId(params object[] args)
		{
			return args.OfType<MainMenuRedirectionArg>().FirstOrDefault()?.StateId;
		}

		private void RegisterState<TState>(params object[] args) where TState : IState
		{
			stateMachine.Add(instantiator.Instantiate<TState>(args));
		}
	}
}