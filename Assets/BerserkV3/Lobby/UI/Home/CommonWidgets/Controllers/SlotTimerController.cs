using System;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Common.UIKit.Abstractions;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Abstractions;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Models;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets.Controllers
{
    public class SlotTimerController : IDisposable
    {
	    private readonly TimerModel timerModel;
	    private readonly Action onTimerEnd;
	    private readonly ISharedTime sharedTime;
	    private readonly IUITimerController uiTimerController;
	    private readonly IUIWidget slotTimerWidget;
	    
	    private bool timerActive;

	    public SlotTimerController(
		    TimerModel timerModel,
		    Action onTimerEnd, 
		    ISharedTime sharedTime, 
		    IUITimerController uiTimerController, 
		    IUIWidget slotTimerWidget)
	    {
		    this.timerModel = timerModel;
		    this.onTimerEnd = onTimerEnd;
		    this.sharedTime = sharedTime;
		    this.uiTimerController = uiTimerController;
		    this.slotTimerWidget = slotTimerWidget;

		    timerActive = true;
		    uiTimerController.OnTick += OnTick;

		    var secondsLeft = (timerModel.EndTime - sharedTime.Current).TotalSeconds;
		    UpdateTime(secondsLeft);
	    }

        private void OnTick()
        {
            if (!timerActive)
                return;

            var currentTime = sharedTime.Current;
            var secondsLeft = (timerModel.EndTime - currentTime).TotalSeconds;

            if (secondsLeft > 0)
            {
                UpdateTime(secondsLeft);
            }
            else
            {
                UpdateTime(0);
                timerActive = false;
                onTimerEnd?.Invoke();
            }
        }

        private void UpdateTime(double secondsLeft)
        {
            var timeSpan = TimeSpan.FromSeconds(secondsLeft);

            var days = timeSpan.Days;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            var timeString = $"{days:D2}{hours:D2}{minutes:D2}{seconds:D2}";
            slotTimerWidget.UpdateTimerLabel(timeString);

            onTimerEnd?.Invoke();
        }

        public void Dispose()
        {
	        if (uiTimerController != null)
	        {
		        uiTimerController.OnTick -= OnTick;
	        }
        }
    }
}
