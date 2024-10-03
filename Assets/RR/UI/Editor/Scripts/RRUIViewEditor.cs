using RR.Core.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace RR.UI.Editor
{
    [CustomEditor(typeof(RRUIView))]
    public class RRUIViewEditor : UnityEditor.Editor
    {
        private static bool isRunning;
        private static bool hasBuildScript;
        private static RRUIView Target;

        protected void OnEnable()
        {
            Target = (RRUIView)target;

            var scriptType = ScriptBuilder.GetTypeByName(Target.name);
            if (scriptType != null && Target.GetComponent(scriptType))
            {
                RRLogger.Error("This object already has generated View. Use REBUILD btn instead.");
                isRunning = false;
                DestroyImmediate(target);
                return;
            }

            if (isRunning)
                return;

            isRunning = true;
            if (Target == null)
            {
                RRLogger.Error("Only RectTransform object can be converted to RR UI View");
                DestroyImmediate(target);
                isRunning = false;
                return;
            }

            hasBuildScript = ScriptBuilder.TryCreateScript(Target.transform as RectTransform);
            if (hasBuildScript)
            {
                if (scriptType != null && !Target.GetComponent(scriptType))
                {
                    isRunning = false;
                    Target.gameObject.AddComponent(scriptType);
                    DestroyImmediate(target);
                }

                return;
            }

            isRunning = false;
            RRLogger.Error("Failed to build script");
            DestroyImmediate(target);
        }

        public override void OnInspectorGUI()
        {
            GUI.Label(new(12, 56, Screen.width, 120), "DO NOT MODIFY!\n\nWait for the Unity to reload assemblies.",
                new()
                {
                    fontSize = 26,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    clipping = TextClipping.Clip,
                    wordWrap = true,
                    stretchHeight = true,
                    stretchWidth = true,
                    normal = new() { textColor = Color.red }
                });
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                if (Target && hasBuildScript)
                {
                    var scriptType = ScriptBuilder.GetTypeByName(Target.name);

                    Target.gameObject.AddComponent(scriptType);

                    DestroyImmediate(Target);
                }

                isRunning = false;
                Target = null;
                hasBuildScript = false;
            };
        }
    }
}