using BerserkV3.Startup.Authorization.ExternalProviders;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Authorization
{
	public interface IExternalProvider
	{
		UniTask<ExternalProviderResponse> LoginAsync();
	}
}