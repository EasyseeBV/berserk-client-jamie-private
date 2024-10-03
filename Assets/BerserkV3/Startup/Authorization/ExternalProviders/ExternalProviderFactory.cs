using Berserk.Shared.Data.Identity.Social;
using Zenject;

namespace BerserkV3.Startup.Authorization.ExternalProviders
{
	public class ExternalProviderFactory : IExternalProviderFactory
	{
		private readonly IInstantiator instantiator;
		public ExternalProviderFactory(IInstantiator instantiator)
		{
			this.instantiator = instantiator;
		}

		public IExternalProvider Create(ExternalProvider provider)
		{
			var args = new object[] {provider};
			return provider switch
			{
				ExternalProvider.Apple => instantiator.Instantiate<ExtenralProviderApple>(args),
				_ => instantiator.Instantiate<ExtenralProviderCommon>(args)
			};
		}
	}
}