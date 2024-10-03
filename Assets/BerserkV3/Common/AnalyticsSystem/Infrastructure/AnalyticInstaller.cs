using Zenject;

namespace BerserkV3.Common.AnalyticsSystem
{
	public class AnalyticInstaller : Installer<AnalyticInstaller>
	{
		public override void InstallBindings()
		{
			InstalPermissionlServices();
			InstallProviders();
			Container
				.BindInterfacesTo<AnalyticsApplication>()
				.AsSingle()
				.Lazy();
		}

		private void InstalPermissionlServices()
		{
#if UNITY_IOS && !UNITY_EDITOR
			Container
				.BindInterfacesTo<ATTrackingPermissionService>()
				.AsSingle()
				.Lazy();
#endif
			
			
#if !UNITY_STANDALONE_OSX
			Container
				.BindInterfacesTo<FirebaseDependencyService>()
				.AsSingle()
				.Lazy();
#endif

		}

		private void InstallProviders()
		{
#if !UNITY_STANDALONE_OSX
			Container
				.BindInterfacesTo<AppsFlyerAnalyticProvider>()
				.AsSingle()
				.Lazy();

			Container
				.BindInterfacesTo<FirebaseAnalyticProvider>()
				.AsSingle()
				.Lazy();
			
			Container
				.BindInterfacesTo<GameAnalyticsProvider>()
				.AsSingle()
				.Lazy();

#if ENABLE_CLOUD_SERVICES_ANALYTICS
			Container
				.BindInterfacesTo<UnityAnalyticProvider>()
				.AsSingle()
				.Lazy();
#endif
#endif
		}
	}
}