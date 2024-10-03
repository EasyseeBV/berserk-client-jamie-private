using Berserk.Shared.Data.Enums;
using RR.Core.EventLayer;

namespace GameCore
{
	public class GameCoreBus : EventBus<GameCoreBus>
	{
		static GameCoreBus()
		{
			InitFields<GameCoreBus>();
		}

		public static void Reset()
		{
			OnLocalTurnPassed?.ClearPrev();
			OnLocalTurnPassed?.Clear();
			OnReconnectRequired = null;
			OnLocalTurnPassed = null;
			OnRequestedChatWheel = null;
			InitFields<GameCoreBus>();
		}

		public static RREvent OnReconnectRequired;
		public static RREvent<string> OnReauthorizationRequired;
		public static State<Owner> OnLocalTurnPassed;
		public static RREvent<string> OnCommandExecute;
		public static State<bool> OnRequestedChatWheel;
	}
}