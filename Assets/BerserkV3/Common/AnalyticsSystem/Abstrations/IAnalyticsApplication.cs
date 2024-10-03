using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.AnalyticsSystem
{

	public interface IAnalyticsApplication
	{
		void InitAbGroup();
		UniTask InitAsync(CancellationToken token);
		void SetUserId(string userId);
	}

}