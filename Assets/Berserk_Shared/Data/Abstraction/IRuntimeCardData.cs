using System;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IRuntimeCardData : IRuntimeData
	{
		int RelativePositionX { get; }
		bool IsToken { get; }
		RuntimeState State { get; }
		RuntimeState PreviousState { get; }
		event Action<RuntimeState, RuntimeState> OnStateChanged;
		event Action<RuntimeState, RuntimeState> OnEarlyStateChanged;
		void TurnToken(bool value);
		void SetRelativePositionX(int value);
		void ResetRelativePositionX();
		void SetState(RuntimeState current, RuntimeState? prev = null);
		void SetStateWithoutNotify(RuntimeState current, RuntimeState? prev = null);
		void TriggerStateChanged();
	}
}