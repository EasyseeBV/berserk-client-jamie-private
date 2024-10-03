using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.GameCore.LogicEventsProcessor;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{
	public class TutorialReceiveEndGameHandler : ITutorialInvokeHandler, ITutorialOrderable, 
		ITutorialCloseHandler, ITutorialIdentity
	{
		private readonly IGameLogicEventsSource logicEventsSource;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private CancellationTokenSource invokeSource;
		public virtual int Order => -1;
		public string[] Ids { get; } =
		{
			TutorialTrigger.ReceiveEndGame.ToString()
		};

		public TutorialReceiveEndGameHandler(
			IGameLogicEventsSource logicEventsSource,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.logicEventsSource = logicEventsSource;
			this.tutorialApplication = tutorialApplication;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			invokeSource ??= new CancellationTokenSource();
			logicEventsSource.Subscribe<EndGame>(() => tutorialApplication.CloseAsync(), invokeSource.Token, int.MaxValue);
			return Task.CompletedTask;
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			invokeSource?.Cancel();
			invokeSource?.Dispose();
			invokeSource = null;

			return Task.CompletedTask;
		}
	}
}