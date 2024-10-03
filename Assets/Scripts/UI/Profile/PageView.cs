using System;
using RR.UI.FrameSystem;

namespace UI
{
	public abstract class PageView : BaseView
	{
		public event Action<string> OnSwitchRequired;
		
		public abstract string Title { get; }
		
		public abstract void InitAndShow();

		protected void SentRequestSwitchPage(string typeName = null)
		{
			OnSwitchRequired?.Invoke(typeName);
		}
	}
}