using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.UserInventory;

namespace BerserkV3.Lobby.Vulcanite.Abstractions
{
	public interface IVulcaniteApplication
	{
		IEnumerable<OwnedVulcanite> Owned { get; }
		OwnedVulcanite Get(string vulcaniteId);
		OwnedVulcanite GetFirstAvailable();
		Task GiveVulcanite(string selectedId);
		bool IsValidVulcanite(string vulcaniteId);
		void RefreshVulcaniteRents(IEnumerable<string> expiredVulcaniteIds);
	}
}