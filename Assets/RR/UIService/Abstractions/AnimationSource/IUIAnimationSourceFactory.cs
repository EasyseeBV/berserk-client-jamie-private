namespace RR.UIService.AnimationSource
{
    public interface IUIAnimationSourceFactory
    {
        IUIAnimationSource Create(IUIWindow window);
    }
}