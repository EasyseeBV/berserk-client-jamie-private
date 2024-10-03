using UnityEngine;

namespace RR.Game.SettingSystem
{
    public interface ISettingsLoader
    {
        void UpdateAll();
        bool HasKey(string key);
        bool GetBool(string key, bool defaultValue = default);
        int GetInt(string key, int defaultValue = default);
        float GetFloat(string key, float defaultValue = default);
        string GetString(string key, string defaultValue = default);
    }
}