using System.Threading.Tasks;
using Berserk.Shared.Data.Customisation;

namespace BerserkV3.Generic.Customisation
{
	public interface ICustomisationApplication
	{
		Task<bool> InitAsync();
		void UnequipItemExept(string id, CustomisationType? type = null);
		void UpdateMainThemeMusic(CustomisationType type);
		Task AcceptChanges();
		void CancelChanges();
	}
}
