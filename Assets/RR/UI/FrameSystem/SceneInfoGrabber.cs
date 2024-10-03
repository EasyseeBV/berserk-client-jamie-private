using RR.UI.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace RR.UI.FrameSystem
{
	public static class SceneInfoGrabber<TK> where TK : Component
	{
		public static List<Type> WellKnownTypes = new List<Type>
		{
			TypeFromDomain("RR.Network.ImageLoader"),
			typeof(RRToggle),
			typeof(RRButton),
			typeof(RRText),
			typeof(TMPro.TextMeshProUGUI),
			typeof(ToggleGroup),
			typeof(CanvasGroup),
			typeof(Button),
			typeof(Toggle),
			typeof(Slider),
			typeof(Scrollbar),
			typeof(TMPro.TMP_Dropdown),
			typeof(TMPro.TMP_InputField),
			typeof(Text),
			typeof(Dropdown),
			typeof(ScrollRect),
			typeof(InputField),
			TypeFromDomain("UnityEngine.UI.ProceduralImage.ProceduralImage"),
			typeof(RawImage),
			typeof(Image),
			//... and last RectTransform:
			typeof(RectTransform)
		}.Where(x => x != null).ToList();

		/// <summary>Build scene model</summary>
		public static IEnumerable<ViewInfo<TK>> GrabInfos(bool onlySpecialNames)
		{
			//get all UniqueId
			return GetUIComponentsOnScene().Select(uid => new ViewInfo<TK>
			{
				Main = uid,
				Name = uid.GetType().Name,
				Members = GrabInfo(uid.transform, onlySpecialNames)
			});
		}

		public static Dictionary<string, Component> GrabInfo(Transform tr, bool onlySpecialNames)
		{
			var res = new Dictionary<string, Component>();

			//get all children (exclude other UniqueId)
			foreach (var comp in FindAllChildren(tr))
			{
				//check name
				var name = comp.gameObject.name;
				if (string.IsNullOrEmpty(name)) continue;

				if (onlySpecialNames && !IsSpecialName(name))
					continue;//ignore Names with first upper symbol

				//prepare name
				name = PrepareName(name, res);

				//get wellknown component
				res[name] = GetWellKnownComponent(comp);
			}

			return res;
		}

		public static bool IsSpecialName(string name)
		{
			return !string.IsNullOrEmpty(name) && char.IsLower(name[0]);
		}

		/// <summary>
		/// Finds active and inactive objects
		/// </summary>
		public static IEnumerable<TK> GetUIComponentsOnScene(bool bOnlyRoot = false)
		{
			var pAllObjects = Resources.FindObjectsOfTypeAll<TK>();

			foreach (var pObject in pAllObjects)
			{
				if (bOnlyRoot)
				{
					if (pObject.transform.parent != null)
					{
						continue;
					}
				}

				if (pObject.hideFlags == HideFlags.NotEditable || pObject.hideFlags == HideFlags.HideAndDontSave)
				{
					continue;
				}

#if UNITY_EDITOR
				if (Application.isEditor)
				{
					string sAssetPath = UnityEditor.AssetDatabase.GetAssetPath(pObject.transform.root.gameObject);
					if (!string.IsNullOrEmpty(sAssetPath))
					{
						continue;
					}
				}
#endif
				yield return pObject;
			}
		}

		/// <summary>Returns all child transfroms (exclude TK and its children)</summary>
		static IEnumerable<Transform> FindAllChildren(Transform parent)
		{
			if (parent == null)
				yield break;

			foreach (Transform child in parent)
			{
				yield return child;

				if (child.GetComponent<TK>() != null)
					continue;

				foreach (var elem in FindAllChildren(child))
					yield return elem;
			}
		}

		static string PrepareName(string name, Dictionary<string, Component> dict)
		{
			name = Regex.Replace(name, @"\W", "");

			//add index if there many items with same name
			var newName = name;
			var counter = 1;
			while (dict.ContainsKey(newName))
			{
				newName = name + counter;
				counter++;
			}

			return newName;
		}

		public static Component GetWellKnownComponent(Transform tr)
		{
			//get all types of components
			var components = tr.GetComponents<Component>();
			var dict = new Dictionary<Type, Component>();
			foreach (var c in components)
				if (c != null)
				{
					dict[c.GetType()] = c;
					if (c is TK)
						return c;
				}

			//select component
			foreach (var t in WellKnownTypes)
				if (dict.TryGetValue(t, out var c))
					return c;

			return tr;
		}

		private static Type TypeFromDomain(string typeName)
			=> AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetType(typeName) != null)?
				.GetType(typeName);
	}

	public class ViewInfo<TK>
	{
		public string Name;
		public TK Main;
		public Dictionary<string, Component> Members = new Dictionary<string, Component>();
	}
}
