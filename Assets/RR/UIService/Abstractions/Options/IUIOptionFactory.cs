namespace RR.UIService.Options
{
    public interface IUIOptionFactory
    {
	    void Init(IUIService service);
	    IUIOptions<T> Create<T>() where T : IUIWindow;
    }
}