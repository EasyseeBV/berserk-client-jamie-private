using BerserkV3.Common.StateMachine;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.Home.States.Collections;
using Zenject;

namespace BerserkV3.Lobby.Home.States
{
	public class GeneralCollectionState : StateWithSubStates
	{
		private readonly IPageSwitcher pageSwitcher;

		public GeneralCollectionState(
			IPageSwitcher pageSwitcher,
			IStateMachine subStateMachine,
			IInstantiator instantiator) 
			: base(subStateMachine, instantiator)
		{
			this.pageSwitcher = pageSwitcher;
			RegisterState<CollectionCardsState>(this);
		}

		public override void OnEnter(params object[] args)
		{
			pageSwitcher.Init(null, null); // TODO init when init a Window
			
			if (TryRedirect(args))
				return;
			
			pageSwitcher.Switch(pageSwitcher.Page);
		}

		protected override void OnRedirected(string stateId, params object[] args)
		{
			pageSwitcher.Switch(stateId, args);
		}
	}
}