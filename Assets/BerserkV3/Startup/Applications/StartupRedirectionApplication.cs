using BerserkV3.Common.Abstractions;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Lobby.MatchMaking.Sessions;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Applications
{
	public class StartupRedirectionApplication : IRedirectionApplication
	{
		private readonly ISceneService sceneService;
		private readonly IGuestApplication guestApplication;
		private readonly IFirstVulcaniteApplication firstVulcaniteApplication;
		private readonly IAuthenticationApplication authenticationApplication;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private readonly ISessionsApplication sessionsApplication;

		public StartupRedirectionApplication(
			ISceneService sceneService,
			IGuestApplication guestApplication,
			IFirstVulcaniteApplication firstVulcaniteApplication,
			IAuthenticationApplication authenticationApplication,
			IBerserkTutorialApplication tutorialApplication,
			ISessionsApplication sessionsApplication)
		{
			this.sceneService = sceneService;
			this.guestApplication = guestApplication;
			this.firstVulcaniteApplication = firstVulcaniteApplication;
			this.authenticationApplication = authenticationApplication;
			this.tutorialApplication = tutorialApplication;
			this.sessionsApplication = sessionsApplication;
		}

		public async UniTask<bool> RedirectAsync()
		{
			if (!await authenticationApplication.AuthenticationRedirectAsync(User.GetRedirections<IAuthArg>()))
				await authenticationApplication.AuthenticationAsync();
			
			User.RemoveRedirections<IAuthArg>();
			await tutorialApplication.InitAsync().AddLoadingTask();
			if (await sessionsApplication.TryConnectGameAsync().AddLoadingTask())
				return true; // redirected
			
			// vulcan only, aren't new accounts can reset a guest
			// or admin reset guest for tests
			if ((!User.IsAnonymous && User.LastLogin.HasValue)
			    || (User.IsAnonymous && !guestApplication.IsSameGuest(User.Id)))
				guestApplication.Reset();
			
			if (!User.IsAnonymous && !User.LastLogin.HasValue) // new account
			{
				// skip tutor after guest registered an account
				if (await guestApplication.ExecuteActionAsync(GuestAction.AfterSignUp, true).AddLoadingTask())
					await firstVulcaniteApplication.SelectFirstVulcanteAsync();
			}
				
			if (tutorialApplication.IsCompleted() 
			    || tutorialApplication.IsCompleted(TutorialTrigger.Start, true))
			{
				await sceneService.LoadAsync(Scene.Lobby).AddLoadingTask();
				return true; // redirected
			}
			
			// tutor is going to handle redirect flow
			await tutorialApplication.InvokeAsync(TutorialTrigger.Start);
			return true; // redirected
		}
	}
}