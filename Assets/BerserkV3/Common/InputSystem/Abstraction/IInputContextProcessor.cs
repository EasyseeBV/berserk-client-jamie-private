namespace BerserkV3.Common.InputSystem
{
	public interface IInputContextProcessor
	{
		IInputContext Process(IInputContext context);
	}
}