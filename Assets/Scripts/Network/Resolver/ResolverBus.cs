using RR.Core.EventLayer;
using Vulcan.Data;

namespace Vulcan.Network.Resolver
{
	public class ResolverBus : EventBus
	{
		static ResolverBus()
		{
			InitFields<ResolverBus>();
			OnUserStateChanged.HideInLog = true;
			OnHandCardAdded.HideInLog = true;
			OnCardPlayed.HideInLog = true;
			OnEntityModified.HideInLog = true;
			OnRoundChanged.HideInLog = true;
			OnPing.HideInLog = true;
		}

		public static void ResetStates()
		{
			OnRoundChanged?.Clear();
		}

		public static RREvent<UserMessage> OnUserStateChanged;

		public static RREvent<GameHandMessage> OnHandCardAdded;
		public static RREvent<GameTableMessage> OnCardPlayed;
		public static RREvent<GameEntityMessage> OnEntityModified;
		public static RREvent<PLayerActionMessage> OnPlayerAction;

		public static State<RoundMessage> OnRoundChanged;
		public static RREvent<TimerGameMessage> OnTimerPaused;
		public static RREvent<TimerGameMessage> OnTimerResumed;

		public static RREvent<RoundMessage> OnMulliganFinished;
		public static RREvent<SessionEndMessage> OnSessionEnd;
		public static RREvent<SessionEndMessage> OnSurrender;

		public static RREvent OnPing;
	}
}