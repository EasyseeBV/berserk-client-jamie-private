using System.Collections.Generic;

namespace BerserkV3.Common.StateMachine
{
    public interface IState
    {
	    int Order { get; set; }
	    string Id { get; }
	    IState ParentState { get; }
	    void OnEnter(params object[] args);
        void OnExit();
        IEnumerable<IState> GetAllStates();
    }
}