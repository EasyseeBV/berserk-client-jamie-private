using System.Threading.Tasks;
using Berserk.Shared.GameCore.Utils;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.CommonHandlers
{
    public class TutorialRaycastConditionHandler : ITutorialInvokeHandler, ITutorialCloseHandler, ITutorialOrderable
    {
        public int Order => 8;
        private static UITutorialBlockRaycasts Window => UITutorialBlockRaycasts.Instance;
        private readonly ITutorialProgressRepository progressRepository;
        private const string AT_INVOKE_TRIGGER = "AtInvokeBlockRaycast";
        private const string AT_CLOSE_TRIGGER = "AtCloseBlockRaycast";
        private bool enabled;
        
        public TutorialRaycastConditionHandler(ITutorialProgressRepository progressRepository)
        {
            this.progressRepository = progressRepository;
        }

        public Task InvokeAsync(ITutorialHintEntity hint)
        {
            if (hint?.Conditions == null
                || !hint.Conditions.TryGet(x=> x.Id == AT_INVOKE_TRIGGER, out var condition)
                || !bool.TryParse(condition.Meta, out enabled))
                return Task.CompletedTask;
            
            Window.Enable(enabled);
            return Task.CompletedTask;
        }

        public Task CloseAsync(ITutorialHintEntity hint)
        {
	        if (progressRepository.IsCompleted() && enabled)
	        {
		        Window.Enable(false);
		        enabled = false;
		        return Task.CompletedTask;
	        }
	        
            if (hint?.Conditions == null
                || !hint.Conditions.TryGet(x=> x.Id == AT_CLOSE_TRIGGER, out var condition)
                || !bool.TryParse(condition.Meta, out enabled))
	            return Task.CompletedTask;
            
            Window.Enable(!progressRepository.IsCompleted() && enabled);
            return Task.CompletedTask;
        }
    }

}