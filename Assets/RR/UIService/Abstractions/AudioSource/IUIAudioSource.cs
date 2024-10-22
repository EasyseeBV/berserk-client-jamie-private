using System;
using System.Collections.Generic;
using RR.UIService.Data;

namespace RR.UIService.AudioSource
{
    public interface IUIAudioSource : IDisposable
    {
        event Action<UIAudioClipData> OnPlayAudio; 
        IEnumerable<IUIAudioHandler> Handlers { get; }
        bool Enabled { get; }
        
        void Enable();
        void Disable();
    }
}