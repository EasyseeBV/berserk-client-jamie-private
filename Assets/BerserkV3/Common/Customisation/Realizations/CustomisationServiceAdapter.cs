namespace BerserkV3.Generic.Customisation
{
	public class CustomisationServiceAdapter
	{
		public static ICustomisationApplication Application { get; private set; }
		public static ICustomisationItemRepository Repository { get; private set; }

		public CustomisationServiceAdapter(
			ICustomisationApplication customisationApplication, 
			ICustomisationItemRepository repository)
		{
			Application = customisationApplication;
			Repository = repository;
		}
	}
}