using System;
using System.IO;
using System.Linq;
using RR.Core.DebugSystem;
using Object = System.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RR.Core.Utilities
{
	public static class FileUtility
    {
        public static string GetFileName(string baseName, string ext, bool useDefaultTimeSpecifier)
        {
            var name = string.Join("_", new[]
                {
                    baseName, useDefaultTimeSpecifier ? $"{DateTime.UtcNow.ToLocalTime():_dd-MM__HH|mm|ss}" : null
                }
                .Where(token => !string.IsNullOrEmpty(token))
            );

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            name = name.Replace("|", "-");
#else
            name = name.Replace("|", ":");
#endif

            if (!string.IsNullOrEmpty(ext))
                name = $"{name}.{ext.TrimStart('.')}";

            return name;
        }
        
        public static void CreateAssetFile(string fileName, string extension, string content, string folderPath, bool pingObject = false)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string path = Path.Combine(folderPath, $"{fileName}{extension}");

            File.WriteAllText(path, content);
            
#if UNITY_EDITOR
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            
            if (pingObject)
            {
                var createdObj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                
                if (createdObj != null)
                    EditorGUIUtility.PingObject(createdObj);
            }
#endif

            RRLogger.Log($"Generated file: {fileName}{extension} at path: {folderPath}");
        }
    }
}
