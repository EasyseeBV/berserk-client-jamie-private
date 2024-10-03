using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers
{
	public interface IEffectCondition
	{
		void Setup(KeywordEffect effect, IGameContext gameContext, IGameLogicContext logicContext);
		bool Eval(params object[] args);
	}
}