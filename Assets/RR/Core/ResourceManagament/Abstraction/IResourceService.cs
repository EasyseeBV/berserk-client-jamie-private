using System;
using System.Threading;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace RR.Core.ResourceManagament
{
	public interface IResourceService
	{
		Task InitializeAsync(IProgress<float> progress = null);
		Task<T> GetAsync<T>(string id, CancellationToken token = default) where T : Object;
		void Release(params object[] values);
	}
}