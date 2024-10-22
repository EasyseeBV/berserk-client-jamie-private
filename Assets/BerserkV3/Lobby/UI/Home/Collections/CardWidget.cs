using BerserkV3.Common.UIKit.OptimizedScroll.Abstractions;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Collections
{
	public class CardWidget : ScrollWidget
	{
		[SerializeField] private Transform parentConter;
		[SerializeField] private GameObject elementPrefab;
	
		private CardWidgetModel widgetModel;
	
		public override void Init(ScrollWidgetModel model)
		{
			TryGetLocalContext(model, out widgetModel);
			if (parentConter.childCount != 0)
				return;
		
			for (var i = 0; i < 5; i++)
			{
				var obj = Instantiate(elementPrefab, parentConter, true);
				obj.gameObject.transform.localScale = Vector3.one;
			}
		}
	}
}