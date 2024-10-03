using Berserk.Shared.Data.Customisation;

namespace BerserkV3.Generic.Customisation
{
	public interface ICustomisationItemFactory
	{
		CustomisationItem Create(CustomisationData data, bool isEquipped);
	}
}
