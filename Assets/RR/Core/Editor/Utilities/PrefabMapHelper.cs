using System.Collections.Generic;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Utilities;
using Sirenix.OdinInspector.Editor;
using System.Linq;
using System.Reflection;
using RR.Core.Serialization;
using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor.Utilities
{
    [CustomEditor(typeof(PrefabMap), true)]
    public class PrefabMapHelper : OdinEditor
    {
        private Texture2D logo;
        private string path;

        protected override void OnEnable()
        {
            base.OnEnable();
            logo = Resources.Load<Texture2D>("RRLogo_600");
            path = "Prefabs";
        }

        private void GrabAllPrefabsFromFolder(string relativePath = "")
        {
            var assets = AssetUtility.GetAssetsAtRelativePath<Component>(relativePath);

            RRLogger.Log($"Found assets:\n{string.Join("\n", assets.Select(x => x.path))}");

            var map = (UnitySerializedDictionary<string, Component>)
                GetMapField.GetValue(target);

            var added = new List<Component>();
            var alreadyAdded = new List<Component>();
            assets.ForEach(x =>
            {
                if (!map.ContainsKey(x.asset.name))
                {
                    var mono = x.asset.GetComponent<MonoBehaviour>();
                    map.Add(x.asset.name, mono ? mono : x.asset);
                    added.Add(x.asset);
                    return;
                }

                if (map[x.asset.name])
                {
                    alreadyAdded.Add(x.asset);
                    return;
                }

                map[x.asset.name] = x.asset.GetComponent<MonoBehaviour>().Value() ?? x.asset;
                if (map[x.asset.name] == null)
                    map.Remove(x.asset.name);
                else 
                    added.Add(map[x.asset.name]);
            });

            if (added.Any())
                RRLogger.Log($"Added to map: {string.Join(", ", added.Select(x => x.name)).Green()}");

            if (alreadyAdded.Any())
                RRLogger.Log($"Already exist in the map: {string.Join(", ", alreadyAdded.Select(x => x.name)).Brown()}");
        }

        private void ClearAll()
        {
            GetMapField.SetValue(target, default);
        }

        private FieldInfo GetMapField =>
            target
                .GetType()
                .GetField(nameof(PrefabMap.Map),
                    BindingFlags.NonPublic
                    | BindingFlags.FlattenHierarchy
                    | BindingFlags.Instance
                    | BindingFlags.IgnoreCase);

        public override void OnInspectorGUI()
        {
            GUI.DrawTexture(new Rect(18, 2, 150, 33), logo, ScaleMode.ScaleToFit, true);
            GUILayout.Space(60);
            GUILayout.Label(new string('_', Screen.width));

            path = GUI.TextField(new Rect(120, 40, 340, 26), path, 300);

            if (GUI.Button(new Rect(18, 40, 96, 26), new GUIContent("Grab prefabs", "Grabs all prefabs inside the folder and setup in map.")))
                GrabAllPrefabsFromFolder(path);

            if (GUI.Button(new Rect(Screen.width - 44, 40, 40, 26), new GUIContent("Clear", "Removes all stored prefabs.")))
                ClearAll();

            DrawDefaultInspector();
        }
    }
}
