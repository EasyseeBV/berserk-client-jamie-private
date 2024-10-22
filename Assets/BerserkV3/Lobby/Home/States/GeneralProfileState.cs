using BerserkV3.Common.StateMachine;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.Home.States.Profile;
using BerserkV3.Lobby.UI.Home.General;
using RR.UIService;
using Zenject;

namespace BerserkV3.Lobby.Home.States
{
	public class GeneralProfileState : StateWithSubStates
	{
		private readonly IUIService uiService;
		private readonly IPageSwitcher pageSwitcher;

		public GeneralProfileState(
			IUIService uiService,
			IPageSwitcher pageSwitcher,
			IInstantiator instantiator,
			IStateMachine subStateMatchine)
			: base(subStateMatchine, instantiator)
		{
			this.uiService = uiService;
			this.pageSwitcher = pageSwitcher;
			RegisterState<ProfileGeneralState>(this);
			RegisterState<ProfileGameplayState>(this);
			RegisterState<ProfileMatchLogState>(this);
			RegisterState<ProfileCollectionState>(this);
		}

		public override void OnEnter(params object[] args)
		{
			uiService.Begin<GeneralProfileWindow>()
				.WithInit(InitWindow)
				.Show();
			
			if (!TryRedirect(args))
				pageSwitcher.Switch(pageSwitcher.Page);
			
			return;

			void InitWindow(GeneralProfileWindow window)
			{
				pageSwitcher.Init(SubStateMachine, window.SwitcherView);
			}
		}

		public override void OnExit()
		{
			uiService.Begin<GeneralProfileWindow>()
				.WithInit(ReleaseWindow)
				.Hide();
			
			base.OnExit();
			return;

			void ReleaseWindow(GeneralProfileWindow window)
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