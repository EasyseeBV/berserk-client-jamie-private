using UnityEngine.UI;

// Disables scrollbar scaling
public class ScrollView : ScrollRect
{
	protected override void LateUpdate()
	{
		base.LateUpdate();

		if (this.horizontalScrollbar)
		{
			this.horizontalScrollbar.size = 0;
		}
	}

	public override void Rebuild(CanvasUpdate executing)
	{
		base.Rebuild(executing);

		if (this.horizontalScrollbar)
		{
			this.horizontalScrollbar.size = 0;
		}
	}
}