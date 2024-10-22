using System.Collections.Generic;

namespace BerserkV3.Common.StateMachine
{
    public abstract class State : IState
    {
	    public int Order { get; set; }
	    public virtual string Id { get; }
        public IState ParentState { get; }
        
        protected State(IState parent = null)
        {
            Id = GetType().Name;
            ParentState = parent;
        }        
        
        protected State(string id)
        {
            Id = id;
        }

        public abstract void OnEnter(params object[] args);

        public virtual void OnExit(){}
        
        public virtual IEnumerable<IState> GetAllStates()
		{
			return new List<IState> {this};
		}
    }
}