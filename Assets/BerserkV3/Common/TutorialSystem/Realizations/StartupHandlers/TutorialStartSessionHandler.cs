using System.Threading.Tasks;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Lobby.MatchMaking.Practice;
using BerserkV3.Startup.Utils;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem.StartupHandlers
{
	public class TutorialSessionStartHandler : ITutorialInvokeHandler, ITutorialOrderable, ITutorialIdentity
	{
		private readonly IBerserkTutorialApplication berserkTutorialApplication;
		private readonly IPracticeApplication practiceApplication;

		public int Order => int.MaxValue;

		public string[] Ids { get; } =
		{
			TutorialTrigger.StartSession.ToString()
		};

		public TutorialSessionStartHandler(
			IBerserkTutorialApplication berserkTutorialApplication,
			IPracticeApplication practiceApplication)
		{
			this.berserkTutorialApplication = berserkTutorialApplication;
			this.practiceApplication = practiceApplication;
		}

		public async Task InvokeAsync(ITutorialHintEntity hint)
		{
			await TaskUtil.RetryLoopAsync(StartTutorialMatchAsync).AddLoadingTask();
			await TaskUtil.RetryLoopAsync(JoinTutorialMatchAsync).AddLoadingTask();
		}

		private async Task<TryResult> StartTutorialMatchAsync()
		{
			if (!await practiceApplication.StartTutorialMatchAsync())
				return TryResult.Retry;

			return TryResult.Success;
		}

		private async Task<TryResult> JoinTutorialMatchAsync()
		{
			if (!await practiceApplication.JoinGameAsync())
				return TryResult.Retry;

			berserkTutorialApplication.CloseAsync().Forget();
			return TryResult.Success;
		}
	}
}