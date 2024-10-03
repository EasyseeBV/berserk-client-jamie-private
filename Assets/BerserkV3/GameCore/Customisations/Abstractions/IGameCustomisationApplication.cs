using System;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.Customisations
{
	public interface IGameCustomisationApplication
	{
		void SubscribeOnReady(Func<UniTask> task);
		
		void SubscribeOnReady(Action task);
		
		T Get<T>(string userId, CustomisationType type) where T : AssetData;
		
		T GetSelf<T>(CustomisationType type) where T : AssetData;
	}
}