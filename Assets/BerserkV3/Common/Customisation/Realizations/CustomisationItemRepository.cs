using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Customisation;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationItemRepository : ICustomisationItemRepository
	{
		private readonly List<CustomisationItem> customizationItems = new();
		private readonly Dictionary<string, bool> appliedValues = new();

		public void AddRange(IEnumerable<CustomisationItem> items)
		{
			if (items == null)
			{
				RRLogger.Error($"[{"CustomizationRepository.AddRange".Red().Bold()}] Range items is missing");
				return;
			}
			
			foreach (var item in items)
			{
				customizationItems.Add(item);
				appliedValues[item.Id] = item.IsEquipped;
			}
		}

		public CustomisationItem Get(string id)
		{
			return customizationItems.FirstOrDefault(x => x.Id == id);
		}

		/// <summary>
		/// By default is CustomizationType.None its any value available in query
		/// </summary>
		/// <param name="customisationType"></param>
		/// <returns></returns>
		public IEnumerable<CustomisationItem> Get(CustomisationType customisationType = CustomisationType.None)
		{
			return customisationType == CustomisationType.None
				? customizationItems.ToList()
				: customizationItems.FindAll(x => x.CustomisationType == customisationType).ToList();
		}

		public IEnumerable<CustomisationItem> GetEquipped(CustomisationType customisationType = CustomisationType.None)
		{
			return Get(customisationType)
				.Distinct()
				.Where(x => x.IsEquipped)
				.ToList();
		}

		public CustomisationItem GetFirstEquipped(CustomisationType customisationType = CustomisationType.None)
		{
			return GetEquipped(customisationType).FirstOrDefault() 
			       ?? Get(customisationType).FirstOrDefault(x => x.IsDefault);
		}

		public bool Any()
		{
			return customizationItems.Any();
		}

		public void Reset()
		{
			customizationItems.ForEach(x =>
			{
				if (appliedValues[x.Id] == true)
					x.Equip();
				else
					x.Unequip();
			});
		}

		public void Save()
		{
			customizationItems.ForEach(x=> appliedValues[x.Id] = x.IsEquipped);
		}
	}
}