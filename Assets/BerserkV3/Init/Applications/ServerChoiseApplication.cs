using BerserkV3.Common.Network;
using BerserkV3.Init.UI;
using BerserkV3.Startup.Network.Enums;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Init.Applications
{
	public interface IServerChoiseApplication
	{
		UniTask InitAsync();
	}

	public class ServerChoiseApplication : IServerChoiseApplication
	{
		private readonly IUIService uiService;

		public ServerChoiseApplication(IUIService uiService)
		{
			this.uiService = uiService;
		}

		public async UniTask InitAsync()
		{
			if (EnvironmentSwitcher.CurrentEnvironment > Environment.Staging)
				return;

			var chooseSource = new UniTaskCompletionSource();
			uiService.Begin<ServerChoiceWindow>()
				.WithInit(InitWindow)
				.Show();

			await chooseSource.Task;
			await uiService.Begin<ServerChoiceWindow>().HideAsync();

			return;

			void InitWindow(ServerChoiceWindow window)
			{
				window.SetSelectedAction(env =>
				{
					EnvironmentSwitcher.SwitchEnvironment(env);
					chooseSource.TrySetResult();
				});
			}
		}
	}
}