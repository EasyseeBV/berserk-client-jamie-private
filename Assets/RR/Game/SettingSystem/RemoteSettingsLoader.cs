using UnityEngine;

namespace RR.Game.SettingSystem
{
    [CreateAssetMenu(fileName = "RemoteSettingsLoader", menuName = "RR/GameSettings/Loaders/UnityRemoteSettings",
        order = 1)]
    public class RemoteSettingsLoader : ScriptableObject, ISettingsLoader
    {
        public void UpdateAll()
        {
            RemoteSettings.ForceUpdate();
        }

        public bool HasKey(string key)
        {
            return RemoteSettings.HasKey(key);
        }
        
        public bool GetBool(string key, bool defaultValue = default)
        {
            return RemoteSettings.GetBool(key, defaultValue);
        }

        public int GetInt(string key, int defaultValue = default)
        {
            return RemoteSettings.GetInt(key, defaultValue);
        }

        public float GetFloat(string key, float defaultValue = default)
        {
            return RemoteSettings.GetFloat(key, defaultValue);
        }

        public string GetString(string key, string defaultValue = default)
        {
            return RemoteSettings.GetString(key, defaultValue);
        }
    }
}
