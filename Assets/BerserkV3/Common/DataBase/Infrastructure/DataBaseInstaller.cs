using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Zenject;

namespace BerserkV3.Common.DataBase
{
	public class DataBaseInstaller : Installer<DataBaseInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<GameDatabase>()
				.AsSingle()
				.NonLazy();

			Container
				.Bind<SharedConfigAdapter>()
				.AsSingle()
				.NonLazy();

			Container
				.Bind<ISharedConfig>()
				.To<SharedConfig>()
				.AsSingle();
			
			Container
				.Bind<GameDataBaseAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}