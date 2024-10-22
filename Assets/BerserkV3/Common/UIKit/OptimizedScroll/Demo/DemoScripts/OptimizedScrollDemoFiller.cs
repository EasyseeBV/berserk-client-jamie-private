using System.Collections.Generic;
using BerserkV3.Common.UIKit.OptimizedScroll.Abstractions;
using UnityEngine;

namespace BerserkV3.Common.UIKit.OptimizedScroll.Demo.DemoScripts
{
	public class OptimizedScrollDemoFiller : MonoBehaviour
	{
		[SerializeField] private Realizations.OptimizedScroll DemoScroll;
		[SerializeField] private int TestCount = 10000;

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
				var widget = new DemoWidgetModel
				{
					DemoNumber = i + 1,
				};
				models.Add(widget);
			}
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				UpdateScroll();
			}
		}

		private void UpdateScroll()
		{
			var modelsRaw = new List<ScrollWidgetModel>();
			var ignoredNumbers = new List<int> { 0, 1, 2, 3, 15, 16, 17, 18, 28, 29 };

			for (int i = 0; i < TestCount; i++)
			{
				if (ignoredNumbers.Contains(i))
					continue;
			
				var widget = new DemoWidgetModel
				{
					DemoNumber = i + 1,
				};
				modelsRaw.Add(widget);
			}
			
			DemoScroll.UpdateContent(modelsRaw);
		}
	}
}
