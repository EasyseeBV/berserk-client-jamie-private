using System;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Utils;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.CommonHandlers
{
    public class TutorialDelyedCloseConditionHandler : ITutorialInvokeHandler
    {
        private readonly IBerserkTutorialApplication application;
        private const string CONDITION_TRIGGER = "DelayedClose";

        public TutorialDelyedCloseConditionHandler(IBerserkTutorialApplication application)
        {
            this.application = application;
        }
        
        public async Task InvokeAsync(ITutorialHintEntity hint)
        {
            if (hint?.Conditions == null
                || !hint.Conditions.TryGet(x => x.Id == CONDITION_TRIGGER, out var condition)
                || !double.TryParse(condition.Meta, out var seconds))
                return;

            if (seconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(seconds));

            await application.CloseAsync();
        }
    }
}