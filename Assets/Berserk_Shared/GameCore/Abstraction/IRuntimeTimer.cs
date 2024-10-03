using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeTimer
	{
		IRuntimeTimerData RuntimeData { get; }
		IRuntimeTimer Sync(IRuntimeTimerData runtimeData);
		IRuntimeTimer SetState(TimerState state, bool notify = true);
		IRuntimeTimer SetOwner(string userId, bool notify = true);
		int GetFullTimeBasedOnState();
		int GetTimeLeft();
		void NextTurn();
		void Pause(bool notify = true);
		void Unpause(bool notify = true);
		void Reset(bool notify = true);
	}
}