using System;
using BerserkV3.Common.UIKit.Abstractions;
using Zenject;

namespace BerserkV3.Common.UIKit
{
	public class UITimerController : IUITimerController, ITickable, IDisposable
	{
		public event Action OnTick;
		void ITickable.Tick()
		{
			OnTick?.Invoke();
		}

		public void Dispose()
		{
			OnTick = null;
		}
	}
}