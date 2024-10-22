using UnityEngine;

namespace BerserkV3.Common.UIKit.OptimizedScroll.Abstractions
{
	public abstract class ScrollWidget : MonoBehaviour
	{
		public abstract void Init(ScrollWidgetModel model);
		/// <summary>
		/// Checking if local context (inherited from ScrollWidget.ScrollWidgetModel class) exist.
		/// </summary>
		protected bool TryGetLocalContext<T>(ScrollWidgetModel model, out T localContext) where T : ScrollWidgetModel
		{
			localContext = null;
            
			if(model != null)
				localContext = model as T;

			return localContext != null;
		}
	}
}