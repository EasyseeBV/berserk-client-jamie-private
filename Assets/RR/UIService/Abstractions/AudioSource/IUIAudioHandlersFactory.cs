namespace RR.UIService.AudioSource
{
    public interface IUIAudioHandlersFactory
    {
        IUIAudioHandler Create(IUIWindow window, IUIAudioConfig config);
    }
}