using System;

namespace RR.Game.TutorialSystem.Domain
{
	public class DelayInvoke
	{
		private float delayBeforeInvoke;
		private float delayAfterInvoke;

		public float DelayBeforeInvoke => delayBeforeInvoke;
		public float DelayAfterInvoke => delayAfterInvoke;

		public DelayInvoke(float delayBeforeInvoke, float delayAfterInvoke)
		{
			if (delayBeforeInvoke < 0)
				TrowException(delayBeforeInvoke);

			if (delayAfterInvoke < 0)
				TrowException(delayAfterInvoke);

			this.delayBeforeInvoke = delayBeforeInvoke;
			this.delayAfterInvoke = delayAfterInvoke;
		}

		private void TrowException(float delay)
		{
			new ArgumentOutOfRangeException(nameof(delay), delay, "Cannot be less than 0");
		}
	}
}