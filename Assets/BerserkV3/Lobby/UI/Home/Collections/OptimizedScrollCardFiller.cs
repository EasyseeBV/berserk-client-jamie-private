using System.Collections.Generic;
using BerserkV3.Common.UIKit.OptimizedScroll.Abstractions;
using BerserkV3.Common.UIKit.OptimizedScroll.Realizations;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Collections
{
	public class OptimizedScrollCardFiller : MonoBehaviour
	{
		[SerializeField] private OptimizedScroll DemoScroll;
		[SerializeField] private int TestCount = 10;
		
		private List<ScrollWidgetModel> models;
		
		private void Awake()
		{
			SetupModels();
			DemoScroll.UpdateContent(models);
		}
		
		private void SetupModels()
		{
			models = new List<ScrollWidgetModel>();
			
			for (int i = 0; i < TestCount; i++)
			{
				var widget = new CardWidgetModel
				{
					DemoNumber = i + 1,
				};
				models.Add(widget);
			}
		}
	}
}