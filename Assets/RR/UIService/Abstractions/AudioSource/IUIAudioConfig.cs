namespace RR.UIService.AudioSource
{
    public interface IUIAudioConfig
    {
        public bool EnabledByDefault { get; }
        public bool ReInitWhenModified { get; }
    }
}