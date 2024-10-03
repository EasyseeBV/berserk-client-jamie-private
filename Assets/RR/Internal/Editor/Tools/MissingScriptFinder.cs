using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace RR.InnerTools.Editor.Tools
{
	public class MissingScriptFinder : OdinEditorWindow
	{
		[MenuItem("RR/Missing Script Resolver")]
		private static void Init()
		{
			GetWindow<MissingScriptFinder>("Missing Script Resolver").Show();
		}

		[SerializeField, ReadOnly]
		private List<GameObject> objectsWithMissingScripts = new List<GameObject>();

		private readonly Func<GameObject[], List<GameObject>> findMissingMembers
			= gameObjects => gameObjects
				.Select(x => (go: x, components: x.GetComponents<Component>()))
				.Where(x => x.components.Any(c => c == null))
				.Select(x => x.go)
				.Distinct()
				.ToList();

		[Button("Search Whole Project", ButtonSizes.Medium)]
		private void SearchProject()
		{
			objectsWithMissingScripts = findMissingMembers(Resources.FindObjectsOfTypeAll<GameObject>());
		}

		[Button("Search Selected Objects", ButtonSizes.Medium)]
		private void SearchSelected()
		{
			if (Selection.gameObjects.Length == 0)
			{
				Debug.LogWarning("No gameObject selected");
				return;
			}

			objectsWithMissingScripts = findMissingMembers(Selection.gameObjects);
		}

		[ShowIf(nameof(IsAnyBroken)), Button("Select All broken", ButtonSizes.Large)]
		private void SelectAll()
		{
			Selection.objects = objectsWithMissingScripts.Select(x => x.gameObject).ToArray();
		}

		private bool IsAnyBroken => objectsWithMissingScripts.Any();
	}
}
