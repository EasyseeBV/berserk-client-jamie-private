using BerserkV3.Common.StateMachine;
using BerserkV3.Lobby.UI.Home.Collections;
using RR.UIService;

namespace BerserkV3.Lobby.Home.States.Collections
{
	public class CollectionCardsState : State
	{
		private readonly IUIService uiService;
		
		public CollectionCardsState(
			IUIService uiService,
			IState parentState)
			: base(parentState)
		{
			this.uiService = uiService;
		}

		public override void OnEnter(params object[] args)
		{
			uiService.Begin<CollectionsCardsWindow>().Show();
		}

		public override void OnExit()
		{
			uiService.Begin<CollectionsCardsWindow>().Hide();
			base.OnExit();
		}
	}
}