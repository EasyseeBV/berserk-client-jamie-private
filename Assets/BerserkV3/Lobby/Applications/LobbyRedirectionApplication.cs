using System.Linq;
using BerserkV3.Common.Abstractions;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.MatchMaking.Leagues.Data;
using BerserkV3.Lobby.UI.Leagues;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Applications;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using UI;

namespace BerserkV3.Lobby.Applications
{
	public class LobbyRedirectionApplication : IRedirectionApplication
	{
		private readonly IBerserkTutorialApplication tutorialApplication;
		private readonly IGuestApplication guestApplication;

		public LobbyRedirectionApplication(
			IBerserkTutorialApplication tutorialApplication, 
			IGuestApplication guestApplication)
		{
			this.tutorialApplication = tutorialApplication;
			this.guestApplication = guestApplication;
		}

			public async UniTask<bool> RedirectAsync()
			{
				if (User.GetRedirections<CampaignRedirectArg>().Any())
				{
					var campaignArg = User.GetRedirections<CampaignRedirectArg>().FirstOrDefault();
					User.RemoveRedirections<CampaignRedirectArg>();

					if (MenuView.Instance != null)
					{
						MenuView.Instance.ShowCampaignAdventures(campaignArg);
						return true;
					}
				}

				if (User.GetRedirections<SoloAdventuresRedirectArg>().Any())
				{
					var soloArg = User.GetRedirections<SoloAdventuresRedirectArg>().FirstOrDefault();
					User.RemoveRedirections<SoloAdventuresRedirectArg>();

					if (MenuView.Instance != null)
					{
						MenuView.Instance.ShowSoloAdventures(soloArg.OpenGauntletSelection);
						return true;
					}
				}

				if (User.GetRedirections<LeagueAutoMatchRedirectArg>().Any())
				{
					var leagueArg = User.GetRedirections<LeagueAutoMatchRedirectArg>().FirstOrDefault();
				User.RemoveRedirections<LeagueAutoMatchRedirectArg>();
				await LobbyLeaguesView.Instance.InitAndShowAsync(leagueArg.LeagueId, leagueArg.DeckId);
				return true;
			}
			
			if (User.IsAnonymous && await guestApplication.ExecuteActionAsync(GuestAction.LobbyLoaded))
				return true;

			if (tutorialApplication.IsCompleted(TutorialTrigger.FirstVulcanite)) 
				return false;
			
			await tutorialApplication.InvokeAsync(TutorialTrigger.FirstVulcanite);
			return true;
		}
	}
}
