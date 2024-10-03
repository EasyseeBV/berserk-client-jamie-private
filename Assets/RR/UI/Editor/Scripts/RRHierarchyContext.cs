using RR.Core.Editor;
using RR.Core.Extensions;
using RR.Core.Utilities;
using RR.UI.Custom;
using RR.UI.DebugSystem;
using RR.UI.FrameSystem;
using UnityEditor;
using UnityEngine;

namespace RR.UI.Editor
{
	// TODO: Consolidate prefab locations
	internal class RRHierarchyContext
	{
		protected const string PredefinedUIPath = "Assets/RR/UI/Predefined/Prefabs/";

		[MenuItem("GameObject/RR/Text", false, -99)]
		public static void CreateNewText()
		{
			AssetUtility.CreateUIElementFromPrefab<RRText>($"{PredefinedUIPath}/{nameof(RRText)}.prefab", "RR Text");
		}

		[MenuItem("GameObject/RR/Button", false, -100)]
		public static void CreateNewButton()
		{
			AssetUtility.CreateUIElementFromPrefab<RRButton>($"{PredefinedUIPath}/{nameof(RRButton)}.prefab", "RR Button");
		}
		
		[MenuItem("GameObject/RR/Toggle", false, -100)]
		public static void CreateNewToggle()
		{
			AssetUtility.CreateUIElementFromPrefab<RRToggle>($"{PredefinedUIPath}/{nameof(RRToggle)}.prefab", "RR Toggle");
		}
		
		[MenuItem("GameObject/RR/UI Manager", false, 300)]
		public static void CreateUIManager()
		{
			var uiManager = Resources.Load<UIManager>("Prefabs/UI Manager");
			uiManager = Object.Instantiate(uiManager);
			uiManager.name = "UI Manager";
		}	
        
		[MenuItem("GameObject/RR/Prefab Map", false, 300)]
		public static void CreatePrefabMap()
		{
            var obj = Object.FindObjectOfType<PrefabMap>();

            if (obj == null)
                obj = new GameObject("Prefab Map").GetOrAddComponent<PrefabMap>();

            Selection.activeGameObject = obj.gameObject;
		}

		[MenuItem("GameObject/RR/Debug Console", false, 500)]
		public static void SpawnConsole()
		{
			var rrConsole = Object.FindObjectOfType<RRConsole>();

			if (rrConsole == null)
				rrConsole = AssetUtility.CreateUIElementFromPrefab<RRConsole>($"{PredefinedUIPath}/{nameof(RRConsole)}.prefab", "RR Console", false);

			Selection.activeGameObject = rrConsole.gameObject;
		}
	}
}
