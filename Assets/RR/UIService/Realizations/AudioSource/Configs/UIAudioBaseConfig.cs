using UnityEngine;

namespace RR.UIService.AudioSource
{
    public abstract class UIAudioBaseConfig : ScriptableObject, IUIAudioConfig
    {
        [SerializeField] private bool enabledByDefault = true;
        [SerializeField] private bool reInitWhenModified;
        
        public bool EnabledByDefault => enabledByDefault;
        public bool ReInitWhenModified => reInitWhenModified;
    }
}