using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IPhaseExecutor
	{
		void ExecutePhase(EffectPhase phase, int targetId);
	}
}