using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.AnalyticsSystem
{

	public interface IPermissionService
	{
		UniTask RequestPermissionAsync(CancellationToken token);
	}

}