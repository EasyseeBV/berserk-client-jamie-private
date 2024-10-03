using BerserkV3.Common.StateMachine;

namespace BerserkV3.Startup.Authorization
{
	public class AuthCompleteState : State
	{
		private readonly IStateMachine stateMachine;

		public AuthCompleteState(IStateMachine stateMachine)
		{
			this.stateMachine = stateMachine;
		}

		public override void OnEnter(params object[] args)
		{
			stateMachine.Switch(EmptyState.Id);
		}
	}
}