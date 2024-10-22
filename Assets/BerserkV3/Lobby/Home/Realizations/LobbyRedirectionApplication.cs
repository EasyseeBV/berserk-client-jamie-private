using System.Linq;
using BerserkV3.Common.Abstractions;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Applications;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Home
{
	public class LobbyRedirectionApplication : IRedirectionApplication
	{
		private readonly IBerserkTutorialApplication tutorialApplication;
		private readonly IGuestApplication guestApplication;
		private readonly IMainMenuApplication mainMenuApplication;

		public LobbyRedirectionApplication(
			IBerserkTutorialApplication tutorialApplication, 
			IGuestApplication guestApplication,
			IMainMenuApplication mainMenuApplication)
		{
			this.tutorialApplication = tutorialApplication;
			this.guestApplication = guestApplication;
			this.mainMenuApplication = mainMenuApplication;
		}

		public async UniTask<bool> RedirectAsync()
		{
			if (User.IsAnonymous && await guestApplication.ExecuteActionAsync(GuestAction.LobbyLoaded))
				return true;

			if (!tutorialApplication.IsCompleted(TutorialTrigger.FirstVulcanite, true))
			{
				await tutorialApplication.InvokeAsync(TutorialTrigger.FirstVulcanite);
				return true;
			}
			
			if (User.HasRedirection<IMainMenuArg>())
			{
				var args = User.GetRedirections<IMainMenuArg>();
				User.RemoveRedirections<IMainMenuArg>();
				mainMenuApplication.Redirect(args.ToArray<object>());
				return true;
			}
			
			mainMenuApplication.Redirect();
			return true;
		}
	}
}