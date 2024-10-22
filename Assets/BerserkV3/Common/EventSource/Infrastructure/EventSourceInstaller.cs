using Zenject;

namespace BerserkV3.Common.EventSource.Infrastructure
{
	public class EventSourceInstaller : Installer<EventSourceInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<EventsSource>()
				.AsSingle();
		}
	}
}