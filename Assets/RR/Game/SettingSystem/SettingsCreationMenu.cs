#if UNITY_EDITOR

using System.Reflection;
using Sirenix.Utilities;
using System.IO;
using RR.Core.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace RR.Game.SettingSystem
{
    internal static class SettingsCreationMenu
    {
        [MenuItem("GameObject/RR/Setting Manager", false, 10)]
        private static void CreateSettings()
        {
            var existingManager = GameObject.FindObjectOfType<SettingsManager>();

            if (existingManager != null)
            {
                RRLogger.Error($"You already have one {nameof(SettingsManager)}!");
                Selection.SetActiveObjectWithContext(existingManager, existingManager);

                return;
            }

            var settingManagerGO = new GameObject("Settings Manager");

            var manager = settingManagerGO.AddComponent<SettingsManager>();
            var settings = SettingsHelper.GetOrCreateSettingsSO();

            var managerType = typeof(SettingsManager);

            var settingsFiled = managerType.GetField("settings", BindingFlags.NonPublic | BindingFlags.Instance);
            settingsFiled.SetMemberValue(manager, settings);
        }
    }
}

#endif