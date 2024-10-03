using System;
using System.Threading.Tasks;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;
using UnityEngine.UI;

namespace BerserkV3.Common.TutorialSystem.CommonHandlers
{
    public class TutorialPopupHandler : ITutorialInvokeHandler, 
        ITutorialCloseHandler, ITutorialRemoveHandler, ITutorialOrderable
    {
	    private static UITutorialPopup Window => UITutorialPopup.Instance;
        private readonly ITutorialTextFormatter textFormatter;
        private readonly IBerserkTutorialApplication tutorialApplication;
        public int Order => 10;
        
        public TutorialPopupHandler(
            ITutorialTextFormatter textFormatter,
            IBerserkTutorialApplication tutorialApplication)
        {
            this.textFormatter = textFormatter;
            this.tutorialApplication = tutorialApplication;
        }

        public async Task InvokeAsync(ITutorialHintEntity hint)
        {
            if (!hint.Popup.Enabled)
                return;

            if (hint.Popup.DelayBeforShow > 0)
                await Task.Delay(TimeSpan.FromSeconds(hint.Popup.DelayBeforShow));
            
            Window.SetBackHinder(hint.Popup.BackHinder);
            Window.SetBackButton(hint.Popup.BackButton, CloseHint);
            Window.SetHeaderText(textFormatter.Format(hint.Popup.Title));
            Window.SetBodyText(textFormatter.Format(hint.Popup.Text));
            Window.SetAcceptButton(hint.Popup.ContinueButton, textFormatter.Format(hint.Popup.ContinueButtonText), CloseHint);
            Window.SetDeclineButton(false);
            Window.Container.anchoredPosition = hint.Popup.Position;

            if (Window.Container.TryGetComponent(out ContentSizeFitter sizeFitter))
	            sizeFitter.enabled = hint.Popup.AutoSize;
            
            if (!hint.Popup.AutoSize)
	            Window.Container.sizeDelta = hint.Popup.SizeDelta;
            
            Window.Enable(hint.Popup.Enabled);
        }

        private void CloseHint()
        {
	        Window.Enable(false);
	        tutorialApplication.CloseAsync().Forget();
        }

        public Task CloseAsync(ITutorialHintEntity hint)
        {
	        if (!hint.Popup.Enabled)
		        return Task.CompletedTask;
	        
	        Window.Enable(false);
	        return Task.CompletedTask;
        }

        public Task RemovedAsync()
        {
	        Window.Enable(false);
	        return Task.CompletedTask;
        }
    }
}