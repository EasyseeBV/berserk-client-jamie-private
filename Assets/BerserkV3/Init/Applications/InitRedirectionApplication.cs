using BerserkV3.Common.Abstractions;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Init.Applications
{
	public class InitRedirectionApplication : IRedirectionApplication
	{
		private readonly ISceneService sceneService;
		public InitRedirectionApplication(ISceneService sceneService)
		{
			this.sceneService = sceneService;
		}

		public UniTask<bool> RedirectAsync()
		{
			sceneService.LoadAsync(Scene.StartUp).AddLoadingTask().Forget();
			return UniTask.FromResult(true);
		}
	}
}