namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public class KeyboardHeightServiceAdapter
	{
		public static IKeyboardHeightService Service { get; private set; }

		public KeyboardHeightServiceAdapter(IKeyboardHeightService service)
		{
			Service = service;
		}
	}
}