using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.InternalTools;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Game.SettingSystem
{
	[DefaultExecutionOrder(-1100)]
    public class SettingsManager : SerializedSingleton<SettingsManager>
    {
        [SerializeField] 
        private GameSettings settings;
        
        [SerializeField] 
        private ISettingsLoader settingsLoader;
        
        [SerializeField] 
        [DictionaryDrawerSettings(IsReadOnly = true)]
        private Dictionary<string, SerializedValue> settingsParameters = new Dictionary<string, SerializedValue>();

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (settings == null)
                return;
            
            settings.OnChanged = ValidateConfigs;

            ValidateConfigs();

            Instance = this;
        }

#endif

        protected override void OnAwake()
        {
            if (settingsParameters.Count != 0) 
                return;
            
            RRLogger.Warning("SettingsParameters dictionary is empty! Check if this field is properly serialized");
            ValidateConfigs();
        }
        
        private void ValidateConfigs()
        {
            settingsParameters.Clear();

            settings.ParameterGroups
                .Where(g => g.Name != null && g.Parameters != null)
                .ForEach(g =>
                {
                    if (settings.ParameterGroups.Count(other => g.Name.Equals(other.Name)) > 1)
                        RRLogger.Error($"Settings already has the group key: [{g.Name}]!");
                    
                    g.Parameters
                        .Where(p => p.Name != null)
                        .ForEach(p =>
                        {
                            var key = SettingsHelper.CreateKey(p.Name, g.Name);

                            if (!SettingsHelper.ValidateKey(key, out var fixedKey))
                                RRLogger.Error($"Invalid key: [{key}]! Fixed to: [{fixedKey}]");

                            if (settingsParameters.ContainsKey(fixedKey))
                                RRLogger.Error($"Settings already has the key: [{fixedKey}]!");
                            else
                                settingsParameters.Add(fixedKey, p.ParameterValue);
                        });
                });
        }

        public static bool HasKey(string key)
        {
            return Instance.settingsParameters.ContainsKey(key);
        }
        
        public static bool GetBool(string key, bool defaultValue = default)
        {
            if (!ContainsKeyCheck(key, out var serializedValue) 
                || !ValueTypeCheck(key, serializedValue, SerializedValueType.Bool))
                return defaultValue;

            if (Instance.settingsLoader == null 
                || !Instance.settingsLoader.HasKey(key))
                return serializedValue.BoolValue;

            return serializedValue.BoolValue = Instance.settingsLoader.GetBool(key, defaultValue);
        }

        public static int GetInt(string key, int defaultValue = default)
        {
            if (!ContainsKeyCheck(key, out var serializedValue) 
                || !ValueTypeCheck(key, serializedValue, SerializedValueType.Int))
                return defaultValue;

            if (Instance.settingsLoader == null 
                || !Instance.settingsLoader.HasKey(key))
                return serializedValue.IntValue;

            return serializedValue.IntValue = Instance.settingsLoader.GetInt(key, defaultValue);
        }

        public static float GetFloat(string key, float defaultValue = default)
        {
            if (!ContainsKeyCheck(key, out var serializedValue) 
                || !ValueTypeCheck(key, serializedValue, SerializedValueType.Float))
                return defaultValue;

            if (Instance.settingsLoader == null 
                || !Instance.settingsLoader.HasKey(key))
                return serializedValue.FloatValue;

            return serializedValue.FloatValue = Instance.settingsLoader.GetFloat(key, defaultValue);
        }

        public static string GetString(string key, string defaultValue = default)
        {
            if (!ContainsKeyCheck(key, out var serializedValue) 
                || !ValueTypeCheck(key, serializedValue, SerializedValueType.String))
                return defaultValue;

            if (Instance.settingsLoader == null 
                || !Instance.settingsLoader.HasKey(key))
                return serializedValue.StringValue;

            return serializedValue.StringValue = Instance.settingsLoader.GetString(key, defaultValue);
        }

        private static bool ContainsKeyCheck(string key, out SerializedValue value)
        {
            if (!Instance.settingsParameters.TryGetValue(key, out value))
            {
                RRLogger.Error($"There is no setting with key: [{key}]!");
                return false;
            }
            return true;
        }

        private static bool ValueTypeCheck(string key, SerializedValue value, SerializedValueType type)
        {
            if (value.ValueType == type)
                return true;

            RRLogger.Error($"Setting with key: [{key}] has type: [{value.ValueType.ToString()}], not: [{type.ToString()}]!");
            return false;
        }
    }
}