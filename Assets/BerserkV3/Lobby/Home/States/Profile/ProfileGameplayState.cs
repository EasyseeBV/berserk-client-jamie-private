using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.StateMachine;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.Vulcanite.Abstractions;
using BerserkV3.Lobby.UI.Home.Profile;
using Cysharp.Threading.Tasks;
using RR.UIService;
using Statistics;

namespace BerserkV3.Lobby.Home.States.Profile
{
	public class ProfileGameplayState : State
	{
		private readonly IUIService uiService;
		private readonly IGameDatabase gameDatabase;
		private readonly ICustomisationItemRepository repository;
		private readonly IStatisticsRepository statisticsRepository;
		private readonly IVulcaniteApplication vulcaniteApplication;

		public ProfileGameplayState(
			IUIService uiService,
			IState parentState, 
			IGameDatabase gameDatabase,
			ICustomisationItemRepository repository,
			IStatisticsRepository statisticsRepository,
			IVulcaniteApplication vulcaniteApplication)
			: base(parentState)
		{
			this.uiService = uiService;
			this.gameDatabase = gameDatabase;
			this.repository = repository;
			this.statisticsRepository = statisticsRepository;
			this.vulcaniteApplication = vulcaniteApplication;
		}

		public override void OnEnter(params object[] args)
		{
			var inGameStat = statisticsRepository.Get("InGameStats");
			var favFactionStat = statisticsRepository.Get("FavoriteFaction");
			var totalGames = int.Parse(inGameStat.Params[0].Get("Value"));
			var totalWins = int.Parse(inGameStat.Params[1].Get("Value"));

			var winrateText = totalWins > 0
				? (totalGames / totalWins) * 100 + "%"
				: "---";

			var hasFavVulcanite = statisticsRepository.Contains("FavoriteVulcanite");
			var favHeroId = hasFavVulcanite
				? statisticsRepository.Get("FavoriteVulcanite").Params.First().Get("Id")
				: vulcaniteApplication.GetFirstAvailable()?.VulcaniteId;
			
			var borderArtUrl = repository.GetFirstEquipped(CustomisationType.AvatarFrame).AssetData.URL;
			var factionArtUrl = GetFactionArtUrl(favFactionStat.Params.First().Get("Id"));
			var vulcaniteArtUrl = GetHeroArtUrl(favHeroId);

			uiService.Begin<ProfileGamePlayWindow>()
				.WithInit(window =>
				{
					window.StatsWidget.Init(totalGames, totalWins, winrateText);
					window.FavoriteFractionWidget.InitAsync(factionArtUrl).Forget(); // token
					window.FavoriteVulcaniteWidget.InitAsync(vulcaniteArtUrl, borderArtUrl).Forget(); // token
				})
				.Show();
		}

		public override void OnExit()
		{
			uiService.Begin<ProfileGamePlayWindow>().Hide();
			base.OnExit();
		}

		private string GetHeroArtUrl(string id)
		{
			return gameDatabase.GetHero(id)?.ArtUrl;
		}

		private string GetFactionArtUrl(string id)
		{
			return $"{id}_Flag";
		}
	}
}