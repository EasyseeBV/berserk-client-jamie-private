using RR.Core.EditorModels;
using RR.Core.Extensions;
using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TypeConstraintAttribute))]
    public class TypeConstraintPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                // Show error
                // Also check that the user only uses the attribute on a GameObject or Component
                // because we need to call GetComponent
            }

            var constraint = attribute as TypeConstraintAttribute;

            if (DragAndDrop.objectReferences.Length > 0)
            {
                var draggedObject = DragAndDrop.objectReferences[0] as GameObject;

                // Prevent dragging of an object that doesn't contain the interface type.
                if (draggedObject == null || draggedObject != null && draggedObject.GetComponent(constraint.Type) == null)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            // If a value was set through other means (e.g. ObjectPicker)
            if (property.objectReferenceValue != null)
            {
                // Check if the interface is present.
                GameObject go = property.objectReferenceValue as GameObject;
                if (go != null && go.GetComponent(constraint.Type) == null)
                {
                    // Clean out invalid references.
                    property.objectReferenceValue = null;
                }
            }

            var newLabel = new GUIContent($"{label.text} :{constraint.Type.HumanReadableName()}");

            property.objectReferenceValue = EditorGUI.ObjectField(position, newLabel, property.objectReferenceValue, typeof(GameObject), true);
        }
    }
}
