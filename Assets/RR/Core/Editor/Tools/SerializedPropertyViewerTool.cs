using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor.Tools
{
    internal class SerializedPropertyViewerTool : EditorWindow
    {
        private class SpData
        {
            public readonly int Depth;
            public readonly string Info;
            public readonly int Oid;

            public SpData(int d, string i, int o)
            {
                if (d < 0)
                {
                    d = 0;
                }

                Depth = d;
                Info = i;
                Oid = o;
            }
        }

        [MenuItem(EditorUtils.ComName + "/SerializedProperty Viewer")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            SerializedPropertyViewerTool window = (SerializedPropertyViewerTool) GetWindow(typeof(SerializedPropertyViewerTool));
            window.titleContent = new GUIContent("SP Viewer");
            window.Show();
        }

        private Object obj;

        private Vector2 scrollPos;
        private List<SpData> data;
        private bool dirty = true;
        private string searchStr = "";
        private string searchStrRep;

        private bool debugMode;
        public static GUIStyle RichTextStyle;


        private void OnGUI()
        {
            if (RichTextStyle == null)
            {
                //EditorStyles does not exist in Constructor??
                RichTextStyle = new GUIStyle(EditorStyles.label);
                RichTextStyle.richText = true;
            }

            Object newObj = EditorGUILayout.ObjectField("Object:", obj, typeof(Object), true);
            debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);
            if (GUILayout.Button("Refresh"))
            {
                obj = null;
            }

            string newSearchStr = EditorGUILayout.TextField("Search:", searchStr);
            if (newSearchStr != searchStr)
            {
                searchStr = newSearchStr;
                searchStrRep = "<color=green>" + searchStr + "</color>";
                dirty = true;
            }

            if (obj != newObj)
            {
                obj = newObj;
                dirty = true;
            }

            if (data == null)
            {
                dirty = true;
            }

            if (dirty)
            {
                dirty = false;
                SearchObject(obj);
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (SpData line in data)
            {
                EditorGUI.indentLevel = line.Depth;
                if (line.Oid > 0)
                {
                    GUILayout.BeginHorizontal();
                }

                EditorGUILayout.SelectableLabel(line.Info, RichTextStyle, GUILayout.Height(20));
                if (line.Oid > 0)
                {
                    if (GUILayout.Button(">>", GUILayout.Width(50)))
                    {
                        Selection.activeInstanceID = line.Oid;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Copy To Clipboard"))
            {
                StringBuilder sb = new StringBuilder();
                Dictionary<int, string> paddingHash = new Dictionary<int, string>();
                string padding = "";
                for (int i = 0; i < 40; i++)
                {
                    paddingHash[i] = padding;
                    padding += " ";
                }

                foreach (SpData line in data)
                {
                    sb.Append(paddingHash[line.Depth]);
                    sb.Append(line.Info);
                    sb.Append("\n");
                }

                EditorGUIUtility.systemCopyBuffer = sb.ToString();
            }
        }

        private void SearchObject(Object obj)
        {
            data = new List<SpData>();
            if (obj == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(obj);
            if (debugMode)
            {
                PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
                inspectorModeInfo?.SetValue(so, InspectorMode.Debug, null);
            }

            SerializedProperty iterator = so.GetIterator();
            Search(iterator, 0);
        }

        private void Search(SerializedProperty prop, int depth)
        {
            LogProperty(prop);
            while (prop.Next(true))
            {
                LogProperty(prop);
            }
        }


        private void LogProperty(SerializedProperty prop)
        {
            string strVal = GetStringValue(prop);
            string propDesc = prop.propertyPath + " type:" + prop.type + " name:" + prop.name + " val:" + strVal + " isArray:" + prop.isArray;
            if (searchStr.Length > 0)
            {
                propDesc = propDesc.Replace(searchStr, searchStrRep);
            }

            data.Add(new SpData(prop.depth, propDesc, GetObjectID(prop)));
        }

        private int GetObjectID(SerializedProperty prop)
        {
            if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null)
            {
                return prop.objectReferenceValue.GetInstanceID();
            }

            return 0;
        }

        private string GetStringValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Character: //this isn't really a thing, chars are ints!
                case SerializedPropertyType.Integer:
                    if (prop.type == "char")
                    {
                        return System.Convert.ToChar(prop.intValue).ToString();
                    }

                    return prop.intValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    if (prop.objectReferenceValue != null)
                    {
                        return prop.objectReferenceValue.ToString();
                    }
                    else
                    {
                        return "(null)";
                    }

                case SerializedPropertyType.Float:
                    return prop.floatValue.ToString(CultureInfo.InvariantCulture);
                default:
                    return "";
            }
        }
    }
}