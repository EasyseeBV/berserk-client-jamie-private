using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;

namespace BerserkV3.Common.UIKit.Editor
{
	[CustomEditor(typeof(ExtendedButton), true)]
	[CanEditMultipleObjects]
	public class ExtendedButtonEditor : ButtonEditor
	{
		private readonly List<SerializedProperty> properties = new();

		protected override void OnEnable()
		{
			base.OnEnable();
			properties.Clear();
			properties.Add(serializedObject.FindProperty("onEnter"));
			properties.Add(serializedObject.FindProperty("onExit"));
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			foreach (var property in properties)
			{
				EditorGUILayout.Space();
				serializedObject.Update();
				EditorGUILayout.PropertyField(property);
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}