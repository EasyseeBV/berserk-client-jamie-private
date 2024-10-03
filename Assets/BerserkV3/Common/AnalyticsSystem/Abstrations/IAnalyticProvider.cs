using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.AnalyticsSystem
{
	public interface IAnalyticProvider
	{
		UniTask InitAsync(CancellationToken token);
		void AddToSendQueue(string eventName, Dictionary<string, object> customData);
		void SetUpAbGroups();
		void SetUserId(string userId);
	}
}