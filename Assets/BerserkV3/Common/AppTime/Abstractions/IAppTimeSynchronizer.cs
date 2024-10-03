using System.Threading.Tasks;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Startup.Utils;

namespace BerserkV3.Common.AppTime
{

	public interface IAppTimeSynchronizer : ISharedTime
	{
		Task<TryResult> SynchronizeTimeAsync();
	}

}