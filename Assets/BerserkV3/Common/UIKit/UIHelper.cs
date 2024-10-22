using System;
using System.Collections.Generic;
using UnityEngine;

namespace BerserkV3.Common.UIKit
{
	public class UIHelper : MonoBehaviour
	{
		/// <summary>
		/// Initializes widgets in a scroll or grid.
		/// </summary>
		/// <param name="widgetsList">List of widgets</param>
		/// <param name="count">Number of widgets to initialize</param>
		/// <param name="initAction">Action to perform on each widget</param>
		public static void InitWidgets<T>(List<T> widgetsList, int count, Action<T, int> initAction) where T : MonoBehaviour
		{
			if (widgetsList == null || widgetsList.Count == 0)
				throw new Exception("You must provide at least one widget!");
			

			if (widgetsList.Count == count)
			{
				InitializeWidgets(widgetsList, initAction);
				return;
			}

			if (widgetsList.Count >= 1 && count > 1) // widgetsList.Count >= 1 left for cases when content need to be updated on same window.
                                            // And we need to update whole grid of widgets
			{
				var widgetParent = widgetsList[0].transform.parent.gameObject;
				SyncChildWidgets(widgetsList[0], widgetParent, count);
				var childWidgets = GetChildWidgets<T>(widgetParent);
				widgetsList.Clear();
				widgetsList.AddRange(childWidgets);
				InitializeWidgets(childWidgets, initAction);
			}
		}
		
		private static void InitializeWidgets<T>(List<T> widgetsList, Action<T, int> initAction) where T : MonoBehaviour
		{
			for (var i = 0; i < widgetsList.Count; i++)
			{
				initAction.Invoke(widgetsList[i], i);
				widgetsList[i].gameObject.SetActive(true);
			}
		}
		private static void SyncChildWidgets<T>(T widgetTemplate, GameObject widgetParent, int count) where T : MonoBehaviour
		{
			var childWidgets = GetChildWidgets<T>(widgetParent);

			if (childWidgets.Count != count)
			{
				RemoveExtraChildWidgets(widgetParent, childWidgets.Count);
				for (var i = 1; i < count; i++)
				{
					InstantiateNewWidget(widgetTemplate.gameObject, widgetParent, i, widgetTemplate.gameObject.name);
				}
			}
		}
	
		private static void RemoveExtraChildWidgets(GameObject widgetParent, int currentCount)
		{
			var widgetsToRemove = new List<GameObject>();
			for (var i = 1; i < currentCount; i++)
			{
				widgetsToRemove.Add(widgetParent.transform.GetChild(i).gameObject);
			}

			foreach (var gameObject in widgetsToRemove)
			{
				DestroyImmediate(gameObject);
			}
		}

		private static void InstantiateNewWidget(GameObject widgetTemplate, GameObject parent, int index, string baseName)
		{
			var newWidgetGo = Instantiate(widgetTemplate, parent.transform, true);
			newWidgetGo.transform.localScale = Vector3.one;
			newWidgetGo.name = $"{index}_{baseName}";
		}
	
		private static List<T> GetChildWidgets<T>(GameObject widgetParent) where T : MonoBehaviour
		{
			var widgets = new List<T>();
			foreach (Transform item in widgetParent.transform)
			{
				var widgetComponent = item.GetComponent<T>();
				if (widgetComponent != null)
					widgets.Add(widgetComponent);
			}

			return widgets;
		}
	}
}