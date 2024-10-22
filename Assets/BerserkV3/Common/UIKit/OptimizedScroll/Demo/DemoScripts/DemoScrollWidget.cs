using BerserkV3.Common.UIKit.OptimizedScroll.Abstractions;
using TMPro;
using UnityEngine;

namespace BerserkV3.Common.UIKit.OptimizedScroll.Demo.DemoScripts
{
	public class DemoScrollWidget : ScrollWidget
	{
		[SerializeField] private TMP_Text itemAmountLabel;
        
		private DemoWidgetModel widgetModel;
        
		public override void Init(ScrollWidgetModel model)
		{
			TryGetLocalContext(model, out widgetModel);
			itemAmountLabel.text = widgetModel.DemoNumber.ToString();
		}
	}
}
