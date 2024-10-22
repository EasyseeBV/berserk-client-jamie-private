using System;
using RR.UIService.Data;

namespace RR.UIService.AudioSource
{
    public interface IUIAudioHandler : IDisposable
    {
        event Action<UIAudioClipData> OnPayAudio;
        bool Enabled { get; }
        void Enable();
        void Disable();
    }
}