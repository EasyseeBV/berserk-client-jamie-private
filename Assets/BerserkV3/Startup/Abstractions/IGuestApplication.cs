using BerserkV3.Startup.Applications;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Abstractions
{
	public interface IGuestApplication
	{
		void Reset();
		bool IsSameGuest(string id);
		UniTask<bool> ExecuteActionAsync(GuestAction action, bool ignorGuest = false);
	}
}