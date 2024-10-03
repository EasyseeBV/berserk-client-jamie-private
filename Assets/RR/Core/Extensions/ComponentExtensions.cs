using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RR.Core.Extensions
{
	public static class ComponentExtensions
	{
		/// <summary>
		/// using null-conditional and null-coalescing operator won't work on Unity objects because it "bypasses the lifetime check on the underlying Unity engine object"
		/// </summary>
		/// https://forum.unity.com/threads/c-null-coalescing-operator-does-not-work-for-unityengine-object-types.543484/
		public static T Value<T>(this T component) where T : Object
		{
			return component ? component : null;
		}

		public static bool HasChildren(this Component component) => component.transform.childCount != 0;

		public static void DestroyGameObject(this Component component, float delay = 0)
		{
			Object.Destroy(component.gameObject, delay);
		}

		public static void Destroy(this Component component, float delay = 0)
		{
			Object.Destroy(component, delay);
		}

		public static void DestroyChildren(this Component component)
		{
			foreach (Transform tr in component.transform)
				Object.Destroy(tr.gameObject);
		}

		public static IEnumerable<Transform> GetChildren(this Component component)
		{
			foreach (Transform tr in component.transform)
				yield return tr;
		}

		public static IEnumerable<RectTransform> GetChildrenRc(this Component component)
		{
			foreach (RectTransform tr in component.transform)
				yield return tr;
		}

		public static IEnumerable<T> GetChildren<T>(this Component component) where T : Component
		{
			foreach (Transform tr in component.transform)
			{
				var res = tr.GetComponent<T>();
				if (res != null)
					yield return res;
			}
		}

		public static void DestroyChildrenImmediate(this Component component)
		{
			foreach (Transform tr in component.transform)
				Object.DestroyImmediate(tr.gameObject);
		}

		public static void DestroyChildren<T>(this Component component) where T : Component
		{
			foreach (Transform tr in component.transform)
				if (tr.GetComponent<T>() != null)
					Object.Destroy(tr.gameObject);
		}

		public static void DestroyChildrenExcept<T>(this Component component) where T : Component
		{
			foreach (Transform tr in component.transform)
				if (tr.GetComponent<T>() == null)
					Object.Destroy(tr.gameObject);
		}
        
        public static void DestroyChildrenImmediateExcept<T>(this Component component) where T : Component
		{
			foreach (Transform tr in component.transform)
				if (tr.GetComponent<T>() == null)
					Object.DestroyImmediate(tr.gameObject);
		}

		public static void DestroyChildrenExcept(this Component component, Transform except)
		{
			foreach (Transform tr in component.transform)
				if (tr != except) Object.Destroy(tr.gameObject);
		}
		
		public static void DestroyChildrenExcept(this Component component, params Transform[] except)
		{
			foreach (Transform tr in component.transform)
				if (!except.Contains(tr)) Object.Destroy(tr.gameObject);
		}

		public static void ForeachChildren<T>(this Component component, Action<T> action) where T : Component
		{
			foreach (Transform tr in component.transform)
			{
				var child = tr.GetComponent<T>();
				if (child != null)
					action(child);
			}
		}

		public static void ForeachChildrenExcept(this Component component, Transform except, Action<Transform> action)
		{
			foreach (Transform child in component.transform)
				if (child != except) action(child);
		}

		public static void ForeachChildren(this Component component, Action<Transform> action)
		{
			foreach (Transform child in component.transform) action(child);
		}

		public static void ForeachChildren(this Component component, Action<Transform, int> action)
		{
			var i = 0;
			foreach (Transform child in component.transform) action(child, i++);
		}

		public static void SetChildrenActive(this Component component, bool active = true)
		{
			foreach (Transform tr in component.transform)
				tr.gameObject.SetActive(active);
		}

		public static T GetOrAddComponent<T>(this Component cmp) where T : Component
			=> GetOrAddComponent<T>(cmp.gameObject);

		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
			=> go.GetComponent<T>().Value() ?? go.AddComponent<T>();

		/// <summary>
		/// Uses reflection to find relevant component type by its name;
		/// Returns null if either type not found or requested type is no of type Component.
		/// Slow. Don't use this in the runtime.
		/// </summary>
		public static Component GetOrAddComponent(this GameObject go, string typeName)
		{
			var compType = AppDomain.CurrentDomain.GetAssemblies()
				.FirstOrDefault(x => x.GetType(typeName) != null)?
				.GetType(typeName);

			if (compType == null || !typeof(Component).IsAssignableFrom(compType))
				return null;

			return go.GetComponent(compType).Value() ?? go.AddComponent(compType);
		}
	}
}