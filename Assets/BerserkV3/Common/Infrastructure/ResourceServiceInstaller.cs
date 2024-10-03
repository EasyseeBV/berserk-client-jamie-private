using RR.Core.ResourceManagament;
using UnityEngine;
using Zenject;

namespace BerserkV3.Generic
{
	public class ResourceServiceInstaller : Installer<ResourceServiceInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<AddressableResourceService>()
				.AsSingle()
				.WithArguments(Application.persistentDataPath)
				.NonLazy();
			
			Container
				.Bind<ResourceServiceAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}