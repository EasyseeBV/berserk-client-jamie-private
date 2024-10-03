using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers
{
	public abstract class EffectConditionBase : IEffectCondition
	{
		protected IGameContext GameContext { get; private set; }
		protected IGameLogicContext LogicContext { get; private set; }
		protected KeywordEffect LinkedEffect { get; private set; }
		
		void IEffectCondition.Setup(KeywordEffect effect, IGameContext gameContext, IGameLogicContext logicContext)
		{
			LinkedEffect = effect;
			GameContext = gameContext;
			LogicContext = logicContext;
		}

		public abstract bool Eval(params object[] args);
	}
}