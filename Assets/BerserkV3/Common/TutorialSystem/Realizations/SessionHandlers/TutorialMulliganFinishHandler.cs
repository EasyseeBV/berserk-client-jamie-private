using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicEvents.AutoBot;
using BerserkV3.GameCore.LogicEventsProcessor;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{
	public class TutorialMulliganFinishHandler : ITutorialInvokeHandler, ITutorialOrderable, ITutorialIdentity
	{
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IBerserkTutorialApplication tutorialApplication;

		public int Order => -1;

		public string[] Ids { get; } =
		{
			TutorialTrigger.MulliganFinish.ToString()
		};

		public TutorialMulliganFinishHandler(
			IGameLogicEventsSource gameLogicEventsSource,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.tutorialApplication = tutorialApplication;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			var subscriptions = new CancellationTokenSource();
			gameLogicEventsSource.Subscribe<MulliganFinished>(() =>
			{
				subscriptions.Cancel();
				subscriptions.Dispose();
				subscriptions = null;
				tutorialApplication.CloseAsync().Forget();
			}, subscriptions.Token);

			return Task.CompletedTask;
		}
	}
}