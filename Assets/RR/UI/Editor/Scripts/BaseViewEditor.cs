using RR.Core.DebugSystem;
using RR.UI.FrameSystem;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace RR.UI.Editor
{
    [CustomEditor(typeof(BaseView), true)]
    public class BaseViewEditor : OdinEditor
    {
        BaseView Target => target as BaseView;
        Texture2D logo;

        protected override void OnEnable()
        {
            base.OnEnable();
            logo = Resources.Load<Texture2D>("RRUI_icon_big");
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(38);
            GUILayout.Label(new string('_', Screen.width));

            if (GUI.Button(new Rect(18, 30, 70, 20), new GUIContent("Open", "Open or Create User Script.")))
                ScriptBuilder.OpenOrCreateUserScript(Target.RectTransform);

            if (GUI.Button(new Rect(88, 30, 70, 20), new GUIContent("Rebuild", "Grab UI elements and rebuild script.")))
                RebuildAutoScript();

            if (GUI.Button(new Rect(158, 30, 70, 20), new GUIContent("Assign", "Automatically find and assign UI elements to their fields if they are nulls.")))
                AssignControls();

            if (GUI.Button(new Rect(158 + 70, 30, 70, 20), new GUIContent("Reassign", "Automatically find and assign UI elements to their fields even if they are already assigned.")))
                AssignControls(true);

            if (GUI.Button(new Rect(Screen.width - 88, 30, 70, 20), new GUIContent("FAQ", "Open wiki.")))
                Application.OpenURL("https://www.notion.so/redrift/RR-Unity-Documentation-WIP-0e5fedcbe9724bb08fe02b4198ac8041");

            GUI.DrawTexture(new Rect(18, 2, 100, 26), logo, ScaleMode.StretchToFill, true);

            DrawDefaultInspector();
        }

        private void AssignControls(bool forced = false)
        {
            var obj = target as BaseView;
            obj?.GrabComponents(forced);
            EditorUtility.SetDirty(obj);

            if (obj != null)
                RRLogger.Log($"{nameof(obj.GrabComponents)} has been performed.");
            else
                RRLogger.Error($"{nameof(target)} is not a BaseView type or null.");
        }

        private void RebuildAutoScript()
        {
            ScriptBuilder.TryCreateScript(Target.RectTransform);
        }
    }
}