using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public abstract class SessionProcessorBase : ISessionProcessor
	{
		public IGameContext Context { get; }
		public IGameLogicContext LogicContext { get; }

		protected SessionProcessorBase(
			IGameContext context, 
			IGameLogicContext logicContext)
		{
			Context = context;
			LogicContext = logicContext;
		}

		public abstract ISessionProcessor Build(params object[] args);
		
		public virtual void Dispose()
		{
			Context?.Dispose();
			LogicContext?.Dispose();
		}
	}
}