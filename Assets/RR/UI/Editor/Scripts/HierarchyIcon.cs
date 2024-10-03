using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RR.UI.Editor
{
    // Implementation taken from https://forum.unity.com/threads/custom-enumeration-class-accessible-through-inspector.440895/

    public class HierarchyIcon : MonoBehaviour, IDropHandler, IHierarchyIcon
    {
        public string Color = "red";
        public string EditorIconPath => $"icon_{Color}";

        public void OnDrop(PointerEventData eventData)
        {
        }

#if UNITY_EDITOR
        [InitializeOnLoad]
        public class HierarchyIcons
        {
            static HierarchyIcons()
            {
                EditorApplication.hierarchyWindowItemOnGUI += EvaluateIcons;
            }

            private static void EvaluateIcons(int instanceId, Rect selectionRect)
            {
                GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                if (go == null) return;

                IHierarchyIcon slotCon = go.GetComponent<IHierarchyIcon>();
                if (slotCon != null) DrawIcon(slotCon.EditorIconPath, selectionRect);
            }

            private static void DrawIcon(string texName, Rect rect)
            {
                Rect r = new Rect(rect.x + rect.width - 16f, rect.y, 15f, 15f);
                GUI.DrawTexture(r, GetTex(texName));

                Texture2D t = new Texture2D(1, 1);
                Color c = new Color(100, 200, 100, 0.1f);
                t.SetPixel(1, 1, c);
                t.Apply();
                GUI.DrawTexture(rect, t, ScaleMode.StretchToFill);
            }

            private static Texture2D GetTex(string name)
            {
                return (Texture2D) Resources.Load("Icons/" + name);
            }
        }
#endif
    }

    public interface IHierarchyIcon
    {
        string EditorIconPath { get; }
    }
}