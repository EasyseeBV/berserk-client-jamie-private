using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{
	public class TutorialEffectPlayConditionHandler : ITutorialInvokeHandler, ITutorialOrderable, 
		ITutorialCloseHandler, ITutorialIdentity
	{
		private readonly IGameLogicEventsSource logicEventsSource;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private CancellationTokenSource invokeSource;
		public virtual int Order => -1;
		public string[] Ids { get; } =
		{
			TutorialTrigger.EffectPlay.ToString()
		};

		public TutorialEffectPlayConditionHandler(
			IGameLogicEventsSource logicEventsSource,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.logicEventsSource = logicEventsSource;
			this.tutorialApplication = tutorialApplication;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (invokeSource != null
			    || hint?.Conditions == null
			    || !hint.Conditions.TryGet(x => Ids.Contains(x.Id), out var condition)
			    || string.IsNullOrEmpty(condition.Meta))
				return Task.CompletedTask;

			invokeSource ??= new CancellationTokenSource();
			logicEventsSource.Subscribe<StartEffect>(EffectStarted, invokeSource.Token);

			void EffectStarted(StartEffect data)
			{
				if (data.RuntimeData.ConfigId != condition.Meta)
					return;

				tutorialApplication.CloseAsync();
			}

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