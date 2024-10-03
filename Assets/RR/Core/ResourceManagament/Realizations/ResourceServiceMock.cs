using System;
using System.Threading;
using System.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Object = UnityEngine.Object;

namespace RR.Core.ResourceManagament
{
	public class ResourceServiceMock : IResourceService
	{
		private void LogUsingMock()
		{
			RRLogger.Log("You trying to use " + $"{nameof(ResourceServiceMock).Red()}");
		}

		public Task InitializeAsync(IProgress<float> progress = null)
		{
			LogUsingMock();
			return Task.CompletedTask;
		}

		public Task<T> GetAsync<T>(string id, CancellationToken token = default) where T : Object
		{
			LogUsingMock();
			return default;
		}

		public void Release(params object[] values)
		{
			LogUsingMock();
		}
	}
}