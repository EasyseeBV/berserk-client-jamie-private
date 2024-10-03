using System.Threading.Tasks;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Abstractions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem.StartupHandlers
{
    public class TutorialWelcomeHandler : ITutorialInvokeHandler, 
	    ITutorialCloseHandler, ITutorialIdentity, ITutorialOrderable
    {
	    private static UITutorialWelcome Window => UITutorialWelcome.Instance;
	    private readonly ISceneService sceneService;
	    private readonly IBerserkTutorialApplication application;
	    private readonly IFirstVulcaniteApplication firstVulcaniteApplication;
	    private readonly ITutorialTextFormatter textFormatter;
        public int Order => int.MaxValue;
        public string[] Ids { get; } =
        {
            TutorialTrigger.Start.ToString()
        };

        public TutorialWelcomeHandler(
	        ISceneService sceneService,
	        IBerserkTutorialApplication application,
	        IFirstVulcaniteApplication firstVulcaniteApplication,
            ITutorialTextFormatter textFormatter)
        {
	        this.sceneService = sceneService;
	        this.application = application;
	        this.firstVulcaniteApplication = firstVulcaniteApplication;
	        this.textFormatter = textFormatter;
        }

        public Task InvokeAsync(ITutorialHintEntity hint)
        {
            if (hint.Popup.Enabled)
	            return Task.CompletedTask;

            Window.SetHeaderText(textFormatter.Format(hint.Popup.Title));
            Window.SetBodyText(textFormatter.Format(hint.Popup.Text));
            Window.SetAcceptButton(hint.Popup.ContinueButton, hint.Popup.ContinueButtonText, CloseHint);
            Window.SetDeclineButton(true, "Skip Tutorial", SkipHintAsync);
            Window.Show();
            return Task.CompletedTask;
        }

        private void CloseHint()
        {
	        application.CloseAsync().Forget();
        }

        private async void SkipHintAsync()
        {
	        await application.SkipAsync().AddLoadingTask();
	        await firstVulcaniteApplication.SelectFirstVulcanteAsync();
	        await sceneService.LoadAsync(Scene.Lobby).AddLoadingTask();
        }

        public Task CloseAsync(ITutorialHintEntity hint)
        {
	        if (hint.Popup.Enabled)
		        return Task.CompletedTask;
	        
	        Window.Close();
	        return Task.CompletedTask;
        }
    }
}