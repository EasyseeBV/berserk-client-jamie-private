using System;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching.Data
{
	public class AutoMatchSearchModel
	{
		private AutoMatchSearchState state = AutoMatchSearchState.Nothing;
		private int timer;
		private bool isTimerOn;
		
		public event Action<AutoMatchSearchState> OnStatusChanged;
		public event Action<int> OnSearchTimeChanged;
		public event Action<bool> OnTimerStateChahnged;
		public string TimeText { get; set; }
		
		public AutoMatchSearchState State
		{
			get => state;
			set
			{
				if (state == value)
					return;
				state = value;
				OnStatusChanged?.Invoke(state);
			}
		}

		public int Timer
		{
			get => timer;
			set
			{
				if (timer == value)
					return;
				timer = value;
				OnSearchTimeChanged?.Invoke(value);
			}
		}

		public bool IsTimerOn
		{
			get => isTimerOn;
			set
			{
				if (isTimerOn == value)
					return;
				isTimerOn = value;
				OnTimerStateChahnged?.Invoke(value);
			}
		}

		public bool IsReverseTimer { get; set; }
		public bool IsTimerIncludeTimout { get; set; }

		public void Dispose()
		{
			OnStatusChanged = null;
			OnSearchTimeChanged = null;
			OnTimerStateChahnged = null;
		}
	}
}