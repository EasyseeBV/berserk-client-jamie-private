using System.Collections.Generic;
using System.IO;
using RR.Core.DebugSystem;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RR.Game.SettingSystem
{
	// todo: make all system internal and place in same spot with .Editor
    public static class SettingsHelper
    {
        public static readonly List<char> AllowedSymbols = new List<char>{'.', '_', '-'};

        public const string KeyValidateMessage = "Key name can't be empty or contains white spaces!";
        
        private const string SettingsSaveFolder = "Assets/Resources";
        private const string SettingFileName = "GameSettings";
        private const string SoExtension = ".asset";
        
        public static bool ValidateKey(string key, out string fixedKey, bool logError = false)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                fixedKey = string.Empty;
                return false;
            }

            fixedKey = string.Copy(key);

            if (!char.IsLetter(key[0]))
            {
                fixedKey.Remove(0);
                
                if (logError)
                    RRLogger.Error("Key names must start with a letter!");
            }
            
            foreach (var symbol in key)
            {
                if (AllowedSymbols.Contains(symbol))
                    continue;

                if (!char.IsLetterOrDigit(symbol))
                {
                    if (logError)
                        RRLogger.Error("Key names may contain only letters, digits, and the characters: “.”, “_”, and “-”!");
                    
                    var symbolStr = symbol.ToString();
                    
                    if (fixedKey.Contains(symbolStr))
                        fixedKey = fixedKey.Replace(symbolStr, string.Empty);
                }
            }
            return true;
        }

        public static string CreateKey(string key, string groupKey)
        {
            return $"{groupKey}.{key}";
        }
        
        #if UNITY_EDITOR
        
        public static GameSettings GetOrCreateSettingsSO()
        {
            string[] paths = AssetDatabase.FindAssets($"t: {typeof(GameSettings).Name}");

            if (paths == null || paths.Length == 0)
            {
                var settings = ScriptableObject.CreateInstance<GameSettings>();
                var path = Path.Combine(SettingsSaveFolder, $"{SettingFileName}{SoExtension}");
                
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.Refresh();

                RRLogger.Log($"Created {nameof(GameSettings)} file at path: [{path}]!");
                
                return settings;
            }

            return AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(paths[0]));
        }
        
        #endif
    }
}

