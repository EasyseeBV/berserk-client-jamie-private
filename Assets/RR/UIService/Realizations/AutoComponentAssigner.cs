#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RR.UIService
{
	public static class AutoComponentAssigner
	{
		public static void ClearAssignedFields(Component parent)
		{
			if (!parent)
				return;

			var baseTypes = typeof(Component);
			var extraAllowed = typeof(GameObject);
			foreach (var field in GetFields(parent, false, baseTypes, extraAllowed))
				field.SetValue(parent, null);
			
			EditorUtility.SetDirty(parent);
		}

		public static void AutoAssignComponents(Component parent)
		{
			if (!parent)
				return;

			var baseTypes = typeof(Component);
			var extraAllowed = typeof(GameObject);
			foreach (var field in GetFields(parent, true, baseTypes, extraAllowed))
			{
				var fieldName = field.Name;
				if (TryFindInChildren(parent.transform, fieldName.ToLower(), out var foundTransform))
				{
					var fieldType = field.FieldType;
					object foundComponent = fieldType == extraAllowed
						? foundTransform.gameObject
						: foundTransform.GetComponent(fieldType);

					if (foundComponent != null)
					{
						field.SetValue(parent, foundComponent);
						Debug.Log(
							$"Component of type {fieldType} assigned to field {fieldName} on {foundTransform.name}");
					}
					else
					{
						Debug.LogWarning($"Component of type {fieldType} not found on object {foundTransform.name}");
					}
				}
				else
				{
					Debug.LogWarning($"Object with name {fieldName} not found in hierarchy");
				}
			}
			
			EditorUtility.SetDirty(parent);
		}

		private static IEnumerable<FieldInfo> GetFields(object obj, bool shouldFieldBeEmpty, params Type[] allowed)
		{
			return obj.GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
				.Where(f => f.IsDefined(typeof(SerializeField), true)
				            && allowed.Any(t => t.IsAssignableFrom(f.FieldType))
				            && f.CheckValue(obj, shouldFieldBeEmpty));
		}

		private static bool CheckValue(this FieldInfo field, object obj, bool shouldFielbBeEmpty)
		{
			var unityObj = (Object) field.GetValue(obj);
			return (!shouldFielbBeEmpty && unityObj) || (shouldFielbBeEmpty && !unityObj);
		}

		private static bool TryFindInChildren(Transform parent, string serchName, out Transform result)
		{
			foreach (Transform child in parent)
			{
				if (child.name.ToLower() != serchName)
				{
					if (!TryFindInChildren(child, serchName, out result))
						continue;

					return true;
				}

				result = child;
				return true;
			}

			result = null;
			return false;
		}
	}
}
#endif