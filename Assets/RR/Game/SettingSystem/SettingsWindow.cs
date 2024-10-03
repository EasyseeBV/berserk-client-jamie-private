#if UNITY_EDITOR

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace RR.Game.SettingSystem
{
    public class SettingsWindow : OdinEditorWindow
    {
        [PropertySpace]
        [SerializeField] 
        private GameSettings settings;

        [PropertySpace]
        [InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.CompletelyHidden)]
        [ShowInInspector]
        private GameSettings settingsPreview
        {
            get => settings;
            set => settings = value;
        }
        
        [MenuItem("RR/Settings window", false, 10)]
        private static void Init()
        {
            SettingsWindow window = (SettingsWindow)EditorWindow.GetWindow(typeof(SettingsWindow));
            window.titleContent = new GUIContent("Settings");

            window.Show();
        }

        protected override void OnEnable()
        {
            if (settings == null)
                settings = SettingsHelper.GetOrCreateSettingsSO();
        }
    }
}

#endif