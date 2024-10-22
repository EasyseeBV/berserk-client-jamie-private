namespace RR.UIService.AudioSource
{
    public interface IUIAudioSourceFactory
    {
        IUIAudioSource Create(IUIWindow window);
    }
}