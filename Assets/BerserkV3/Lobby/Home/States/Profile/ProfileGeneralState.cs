using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.StateMachine;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.Vulcanite.Abstractions;
using BerserkV3.Lobby.UI.Home.Profile;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.Extensions;
using RR.UIService;
using Statistics;

namespace BerserkV3.Lobby.Home.States.Profile
{
	public class ProfileGeneralState : State
	{
		private readonly IVulcaniteApplication vulcaniteApplication;
		private readonly IStatisticsRepository statisticsRepository;
		private readonly IUIService uiService;
		private readonly IGameDatabase gameDatabase;
		private readonly ICustomisationItemRepository custRepository;

		public ProfileGeneralState(
			IStatisticsRepository statisticsRepository, 
			IVulcaniteApplication vulcaniteApplication,
			IUIService uiService, 
			ICustomisationItemRepository repository, 
			IGameDatabase gameDatabase,
			IState parentSate) : base(parentSate)
		{
			this.statisticsRepository = statisticsRepository;
			this.vulcaniteApplication = vulcaniteApplication;
			this.uiService = uiService;
			this.gameDatabase = gameDatabase;
			this.custRepository = custRepository;
		}

		public override void OnEnter(params object[] args)
		{
			var mmrStat = statisticsRepository.Get("MMR");
			var leagues = mmrStat.Params.ToDictionary(param => GetLeagueArtUrl(param.Get("Id")), param => param.Get("Value"));
			var favFaction = statisticsRepository.Get("FavoriteFaction");
			var favVulncaiteId = statisticsRepository.Contains("FavoriteVulcanite")
				? statisticsRepository.Get("FavoriteVulcanite").Params.First().Get("Id")
				: vulcaniteApplication.GetFirstAvailable()?.VulcaniteId;
			
			var favDeckCardIds = statisticsRepository.Contains("FavoriteDeck")
				? statisticsRepository.Get("FavoriteDeck").Params.Select(x => x.Get("Id")).ToArray()
				: Array.Empty<string>();
			var favDeckCards = gameDatabase.GetCards(favDeckCardIds).ToArray();
			
			var borderId = custRepository.GetFirstEquipped(CustomisationType.AvatarFrame).AssetData.URL;
			var vulcaniteArtUrl = GetHeroArtUrl(favVulncaiteId);
			var factionArtUrl = GetFactionArtUrl(favFaction.Params.First().Get("Id"));
			var userName = User.UserName.Ellipsis(16);
			
			uiService.Begin<ProfileGeneralWindow>()
				.WithInit(window =>
				{
					window.MmrWidget.Clear();
					foreach (var (artUrl, mmrValue) in leagues)
						window.MmrWidget.AddLeagueAsync(artUrl, mmrValue).Forget(); // token
					
					window.DeckWidget.Enable(favDeckCards.Length > 0);
					window.DeckWidget.InitAsync(favDeckCards).Forget(); // token
					
					window.FavoriteFactionWidget.InitAsync(factionArtUrl).Forget(); // token
					window.FavoriteVulcaniteWidget.InitAsync(vulcaniteArtUrl, borderId).Forget(); // token
					window.ProfileWidget.InitAsync(userName, vulcaniteArtUrl, borderId).Forget(); // token
				})
				.Show();
		}

		public override void OnExit()
		{
			uiService.Begin<ProfileGeneralWindow>().Hide();
			base.OnExit();
		}

		private string GetHeroArtUrl(string id)
		{
			return gameDatabase.GetHero(id)?.ArtUrl;
		}
		
		private string GetLeagueArtUrl(string paramKey)
		{
			if (!int.TryParse(paramKey, out var index)
			    || index < 0
			    || index >= LobbyBus.Leagues.Value.Count)
			{
				var model = LobbyBus.Leagues.Value.FirstOrDefault(x => x.Id == paramKey)
				            ?? throw new InvalidOperationException("League key is missing");
				return model.GetArtURL();
			}

			return LobbyBus.Leagues.Value[index].GetArtURL();
		}
		
		private string GetFactionArtUrl(string id)
		{
			return $"{id}_Flag";
		}
	}
}