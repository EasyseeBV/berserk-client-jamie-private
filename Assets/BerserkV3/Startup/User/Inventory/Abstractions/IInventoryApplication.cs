using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;

namespace BerserkV3.Startup.Authorization.Inventory.Models
{
	public interface IInventoryApplication
	{
		Task<bool> InitAsync();
		T Get<T>(string id) where T : IInventoryItem;
		T[] Get<T>() where T : IInventoryItem;
		void Remove(IInventoryItem item);
		void Add(IInventoryItem item);
		string ValidateUserData(bool logger);
	}
}