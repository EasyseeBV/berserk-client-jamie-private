using RR.Core.Editor;
using UnityEditor;

namespace RR.UI.Editor
{
	internal class UIRebuildTool : EditorWindow
	{
		[MenuItem(EditorUtils.ComName + "/UI" + "/Rebuild all")]
		private static void RebuildAll()
		{
			ScriptBuilder.RebuildAll();
		}
	}
}