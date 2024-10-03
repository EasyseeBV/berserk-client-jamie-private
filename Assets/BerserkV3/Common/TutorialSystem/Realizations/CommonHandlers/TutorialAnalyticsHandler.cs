using System;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Lobby;
using BerserkV3.Startup.Authorization;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.CommonHandlers
{

	public class TutorialAnalyticsHandler : ITutorialInvokeHandler, ITutorialIdentity
	{
		private const string TUTORIAL_BEGIN = nameof(TutorialTrigger.StartSession);
		private const string TUTORIAL_COMPLETED = nameof(TutorialTrigger.TutorialEnd);

		private readonly IBerserkAnalyticsApplication analyticsApplication;
		private readonly ISharedTime sharedTime;
		private DateTime startedTime;
		
		public string[] Ids { get; } =
		{
			TUTORIAL_BEGIN,
			TUTORIAL_COMPLETED
		};
		
		public TutorialAnalyticsHandler(
			ISharedTime sharedTime,
			IBerserkAnalyticsApplication analyticsApplication)
		{
			this.sharedTime = sharedTime;
			this.analyticsApplication = analyticsApplication;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			var isFirstPlay = !analyticsApplication.AnalyticsData.TutorialCompleted;
			
			if (hint.TriggerIds.Contains(TUTORIAL_BEGIN))
			{
				startedTime = sharedTime.Current;
				AnalyticsBus.SendCustomEvent.Publish(new TutorialBeginModel(User.UserName, isFirstPlay));
			}
			
			if (hint.TriggerIds.Contains(TUTORIAL_COMPLETED))
			{
				var elapsedTime = (sharedTime.Current - startedTime).ToString("hh':'mm':'ss");
				AnalyticsBus.SendCustomEvent.Publish(new TutorialCompletedModel(User.UserName, isFirstPlay, elapsedTime));
			}
			
			return Task.CompletedTask;
		}
	}

}