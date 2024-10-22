using System;

namespace BerserkV3.Common.UIKit.Abstractions
{
	public interface IUITimerController
	{
		event Action OnTick;
	}
}