using Audio;
using RR.Core.EventLayer;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationBus : EventBus
	{
		static CustomisationBus()
		{
			InitFields<CustomisationBus>();
		}
		
		public static State<Clip> OnMusicUpdated;
	}
}