using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.StateMachine;
using Zenject;

namespace BerserkV3.Lobby.Home.States
{
	public class GeneralHomeState : StateWithSubStates
	{
		public GeneralHomeState(
			IStateMachine subStateMachine,
			IInstantiator instantiator)
			: base(subStateMachine, instantiator)
		{
		}

		public override void OnEnter(params object[] args)
		{
			DefaultSharedLogger.Error($"Not implemented {nameof(OnEnter)}");
		}
	}
}