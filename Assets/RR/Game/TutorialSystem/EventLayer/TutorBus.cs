using RR.Core.EventLayer;
using RR.Game.TutorialSystem.Domain;
using RR.Game.TutorialSystem.Domain.Data;

namespace RR.Game.TutorialSystem.Event
{
	public class TutorBus : EventBus
	{
		static TutorBus()
		{
			InitFields<TutorBus>();
			OnHintInvoked.Assign(default);
			OnHintInvoked.HideInLog = true;

			OnHintClosed.Assign(default);
			OnHintClosed.HideInLog = true;
		}

		public static State<TutorHintEntity> OnHintInvoked;
		public static State<TutorHintEntity> OnHintClosed;
		public static RREvent<string> OnPlaceVisited;

		public static RREvent<TutorDynamicArrowData> OnTutorArrowShowed;
		public static RREvent OnTutorArrowHidden;
	}
}