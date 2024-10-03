using Berserk.Shared.Data.Identity.Social;

namespace BerserkV3.Startup.Authorization
{
	public interface IExternalProviderFactory
	{
		IExternalProvider Create(ExternalProvider provider);
	}
}