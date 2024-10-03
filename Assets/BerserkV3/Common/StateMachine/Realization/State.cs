namespace BerserkV3.Common.StateMachine
{
    public abstract class State : IState
    {
        public virtual string Id { get; }
        
        protected State()
        {
            Id = GetType().Name;
        }        
        
        protected State(string id)
        {
            Id = id;
        }

        public abstract void OnEnter(params object[] args);

        public virtual void OnExit(){}
    }
}