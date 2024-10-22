namespace RR.UIService.AnimationSource
{
    public interface IUIAnimationsFactory
    {
        IUIAnimation Create(IUIWindow window, IUIAnimationConfig config);
    }
}