using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.StateMachine;

namespace BerserkV3.Lobby.Home.States.GameModes
{
	public class GameModeDuelsState : State
	{
		public GameModeDuelsState(IState parentState) : base(parentState) {}

		public override void OnEnter(params object[] args)
		{
			DefaultSharedLogger.Error($"Not implemented {nameof(OnEnter)}");
		}

		public override void OnExit()
		{
			base.OnExit();
		}
	}
}