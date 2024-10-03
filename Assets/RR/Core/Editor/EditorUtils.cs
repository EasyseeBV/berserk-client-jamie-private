using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor
{
	public static class EditorUtils
	{
		public const string ComName = "RR";
		public const float ElementHeight = 20f;
		public const float Padding = 6f;

		public static void LineSeparator(int height = 1)
		{
			Rect rect = EditorGUILayout.GetControlRect(false, height);
			rect.height = height;
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
		}
	}
}
