using BerserkV3.Common.StateMachine;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.Home.States.GameModes;
using BerserkV3.Lobby.UI.Home.General;
using RR.UIService;
using Zenject;

namespace BerserkV3.Lobby.Home.States
{
	public class GeneralGameModesState : StateWithSubStates
	{
		private readonly IUIService uiService;
		private readonly IPageSwitcher pageSwitcher;
		
		public GeneralGameModesState(
			IUIService uiService,
			IPageSwitcher pageSwitcher,
			IStateMachine subStateMachine,
			IInstantiator instantiator)
			: base(subStateMachine, instantiator)
		{
			this.uiService = uiService;
			this.pageSwitcher = pageSwitcher;
			RegisterState<GameModeLeaguesState>(this);
			RegisterState<GameModeDraftState>(this);
			RegisterState<GameModePracticeState>(this);
			RegisterState<GameModeDuelsState>(this);
		}

		public override async void OnEnter(params object[] args)
		{
			await uiService
				.Begin<GeneralGameModesWindow>()
				.WithInit(InitWindow)
				.ShowAsync();
			
			if (TryRedirect(args))
				return;

			pageSwitcher.Switch(pageSwitcher.Page);
			
			return;
			void InitWindow(GeneralGameModesWindow window)
			{
				pageSwitcher.Init(SubStateMachine, window.SwitcherView);
				pageSwitcher.OnPageSwitched += UpdateHeader;
				
				return;
				void UpdateHeader(int pageIndex)
				{
					window.HeaderWidget.SetText(pageSwitcher.GetPageName(pageIndex));
				}
			}
		}

		public override async void OnExit()
		{
			await uiService
				.Begin<GeneralGameModesWindow>()
				.WithInit(ReleaseWindow)
				.HideAsync();
			
			base.OnExit();
			
			return;
			void ReleaseWindow(GeneralGameModesWindow window)
			{
				pageSwitcher.Release();
			}
		}

		protected override void OnRedirected(string stateId, params object[] args)
		{
			pageSwitcher.Switch(stateId, args);
		}
	}
}