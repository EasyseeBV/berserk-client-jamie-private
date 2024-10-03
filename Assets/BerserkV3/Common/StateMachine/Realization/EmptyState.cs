namespace BerserkV3.Common.StateMachine
{
    public class EmptyState : State
    {
        public new static string Id => nameof(EmptyState);
        public EmptyState() : base(nameof(EmptyState)){}
        public override void OnEnter(params object[] args) {}
        public override void OnExit() {}
    }
}