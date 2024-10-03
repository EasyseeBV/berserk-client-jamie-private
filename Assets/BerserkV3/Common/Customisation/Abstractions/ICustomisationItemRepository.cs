using System.Collections.Generic;
using Berserk.Shared.Data.Customisation;

namespace BerserkV3.Generic.Customisation
{
	public interface ICustomisationItemRepository
	{
		void AddRange(IEnumerable<CustomisationItem> customizationItems);

		CustomisationItem Get(string id);
		
		IEnumerable<CustomisationItem> Get(CustomisationType customisationType = CustomisationType.None);

		IEnumerable<CustomisationItem> GetEquipped(CustomisationType customisationType = CustomisationType.None);

		CustomisationItem GetFirstEquipped(CustomisationType customisationType = CustomisationType.None);
		
		bool Any();
		
		void Reset();
		
		void Save();
	}
}