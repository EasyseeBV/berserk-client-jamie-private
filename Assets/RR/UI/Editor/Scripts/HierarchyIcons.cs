using RR.UI.FrameSystem;
using UnityEditor;
using UnityEngine;

namespace RR.UI.Editor
{
	[InitializeOnLoad]
	class HierarchyIcons
	{
		static Texture2D rrLogo;
		static Texture2D rrManagerLogo;

		static HierarchyIcons()
		{
			rrLogo = Resources.Load<Texture2D>("RRUI_icon");
			rrManagerLogo = Resources.Load<Texture2D>("RRUI_icon_manager");
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
		}

		static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			var r = new Rect(selectionRect) { x = 32, width = 20 };
			var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

			if (!go)
				return;

			if (go.GetComponent<BaseView>())
				GUI.Label(r, rrLogo);

			if (go.GetComponent<UIManager>())
				GUI.Label(r, rrManagerLogo);
		}
	}
}