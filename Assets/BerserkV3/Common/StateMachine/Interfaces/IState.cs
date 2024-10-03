namespace BerserkV3.Common.StateMachine
{
    public interface IState
    {
	    string Id { get; }
        void OnEnter(params object[] args);
        void OnExit();
    }
}