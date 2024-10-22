using UnityEditor;
using UnityEditor.UI;

namespace BerserkV3.Common.UIKit.Editor
{
	[CustomEditor(typeof(ExtendedToggle), true)]
	public class ExtendedToggleEditor : ToggleEditor
	{
		SerializedProperty textLabelProperty;
		SerializedProperty childImageProperty;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			textLabelProperty = serializedObject.FindProperty("textLabel");
			childImageProperty = serializedObject.FindProperty("selector");
		}
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(textLabelProperty);
			EditorGUILayout.PropertyField(childImageProperty);
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}