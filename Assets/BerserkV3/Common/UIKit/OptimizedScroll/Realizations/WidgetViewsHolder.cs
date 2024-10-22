using BerserkV3.Common.UIKit.OptimizedScroll.Abstractions;
using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace BerserkV3.Common.UIKit.OptimizedScroll.Realizations
{
	// This class keeps references to an item's views.
	// Your views holder should extend BaseItemViewsHolder for ListViews and CellViewsHolder for GridViews
	public class WidgetViewsHolder : BaseItemViewsHolder
	{
		public ScrollWidget Widget;

		// Retrieving the views from the item's root GameObject
		public override void CollectViews()
		{
			base.CollectViews();

			// GetComponentAtPath is a handy extension method from frame8.Logic.Misc.Other.Extensions
			// which infers the variable's component from its type, so you won't need to specify it yourself
			root.GetComponentAtPath("Widget", out Widget);
		}

		// Override this if you have children layout groups or a ContentSizeFitter on root that you'll use. 
		// They need to be marked for rebuild when this callback is fired
		/*
		public override void MarkForRebuild()
		{
			base.MarkForRebuild();

			LayoutRebuilder.MarkLayoutForRebuild(yourChildLayout1);
			LayoutRebuilder.MarkLayoutForRebuild(yourChildLayout2);
			YourSizeFitterOnRoot.enabled = true;
		}
		*/

		// Override this if you've also overridden MarkForRebuild() and you have enabled size fitters there (like a ContentSizeFitter)
		/*
		public override void UnmarkForRebuild()
		{
			YourSizeFitterOnRoot.enabled = false;

			base.UnmarkForRebuild();
		}
		*/
	}
}