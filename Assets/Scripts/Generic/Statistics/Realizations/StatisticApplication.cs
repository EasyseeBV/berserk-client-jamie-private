using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Berserk.Shared.Data.Lobby.Statistics;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Network;
using Lobby;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;

namespace Statistics
{
	public class StatisticApplication : IStatisticApplication
	{
		private readonly IInstantiator instantiator;
		private readonly IStatisticsRepository repository;
		private const float UPDATE_INTERVAL = 500;
		private const string DEFAULT_ELO = "1200"; // TODO GameConfig.BaseMMR move to SharedConfig and use here
		private float lastUpdateTime;
		private bool RequiredRefresh => lastUpdateTime < Time.time;

		public StatisticApplication(IStatisticsRepository repository, ISharedConfig sharedConfig)
		{
			this.repository = repository;
		}

		public async Task<bool> Init()
		{
			if (!RequiredRefresh)
				return true;
			
			var statisticsResponse = await PlayerAPI.GetStatistics().AddLoadingTask();

			if (!statisticsResponse.IsSuccess)
				return false;

			var leaguesRequest = new TaskCompletionSource<List<LeagueModel>>();
			LobbyBus.Leagues.SubscribeRaw(LeaguesUpdate);
			LobbyBus.LeaguesRefereshRequered.Publish();

			void LeaguesUpdate(List<LeagueModel> leagues)
			{
				LobbyBus.Leagues.Unsubscribe(LeaguesUpdate);
				leaguesRequest.SetResult(leagues);
			}

			var leagues = await leaguesRequest.Task.AddLoadingTask();
			if (leagues.IsNullOrEmpty())
				return false;

			repository.Clear();
			lastUpdateTime = Time.time + UPDATE_INTERVAL;
			var statisticsModel = statisticsResponse.Data ?? new UserStatisticsData
			{
				FavDeck = new UserStatisticDeckData { CardIds = new List<string>() },
				TimePlayedInSeconds = 0,
				VulcaniteStats = new List<UserStatisticsHeroData>(),
				LeagueStats = new List<UserStatisticsLeagueData>()
			};
			var favDeckCards = statisticsModel.FavDeck?.CardIds ?? new List<string>();
			var statisticMap = new Dictionary<string, List<IStatisticParam>>
			{
				{
					"FavoriteDeck", favDeckCards
						.Distinct()
						.Select(cardId => CreateParam(("Id", cardId),("Count", favDeckCards.Count(c => c == cardId).ToString())))
						.ToList()
				},

				{"FavoriteFaction", new List<IStatisticParam>  {CreateParam("Id", FilterFaction(statisticsModel.FavLandId))}},
				{"FavoriteVulcanite", new List<IStatisticParam> {CreateParam("Id", statisticsModel.FavVulcaniteId)}},

				{
					"InGameStats", new List<IStatisticParam>
					{
						CreateParam(("Title", "Total Games :"), ("Value", statisticsModel.Games.ToString())),
						CreateParam(("Title", "Total Wins :"), ("Value", statisticsModel.Wins.ToString())),
						CreateParam(("Title", "Total Losses :"), ("Value", statisticsModel.Losses.ToString())),
						CreateParam(("Title", "Total Hours :"), ("Value", TimeSpan.FromSeconds(statisticsModel.TimePlayedInSeconds).Hours.ToString())),
					}
				},

				{"PlayerOne", GetSortedVulcaniteParams(0, statisticsModel.VulcaniteStats)},
				{"PlayerTwo", GetSortedVulcaniteParams(1, statisticsModel.VulcaniteStats)},

				{
					"MMR", leagues
						.Select(model => CreateParam(("Id", model.Id), ("Value", statisticsModel
							.LeagueStats
							.FirstOrDefault(x => x.LeagueId == model.Id)?
							.ELO
							.ToString() ?? DEFAULT_ELO)))
						.ToList()
				}
			};

			statisticMap.ForEach(kpv =>
			{
				var statisticParams = kpv.Value.Where(x => x != null).ToArray();
				if (statisticParams.Length == 0)
					return;

				repository.Add(new StatisticModel(kpv.Key, statisticParams));
			});
			return true;
		}

		public bool AvailableStatistic(string id)
		{
			return repository.Contains(id);
		}

		public void Dispose()
		{
			repository.Clear();
		}
		
		private string FilterFaction(string id)
		{
			return InValidParam(id, id) || id == Quadrant.Neutral.ToString() 
				? null 
				: id;
		}

		private List<IStatisticParam> GetSortedVulcaniteParams(int playerIndex, IEnumerable<UserStatisticsHeroData> models)
		{
			return models?
				.Where(x => x.StartTurn == playerIndex)
				.OrderByDescending(x => GetPercent(x.Games, x.Wins))
				.Distinct()
				.Select(x => CreateParam(("Id", x.VulcaniteId), ("Value", GetPercent(x.Games, x.Wins).ToString())))
				.ToList() ?? new List<IStatisticParam>();
		}

		private IStatisticParam CreateParam(string id, string value)
		{
			return InValidParam(id, value)
				? null
				: new StatisticParam(id, value);
		}

		private IStatisticParam CreateParam(params ValueTuple<string, string>[] values)
		{
			if (values == null
				|| values.Length == 0
				|| values.Any(param => InValidParam(param.Item1, param.Item2)))
				return null;

			return new StatisticParam(values);
		}

		private bool InValidParam(string value1, string value2)
		{
			return string.IsNullOrEmpty(value1) 
			       || string.IsNullOrEmpty(value2) 
			       || value1 == "null" 
			       || value2 == "null";
		}

		private int GetPercent(int total, float current)
		{
			return (int)(current / total * 100);
		}
		
		private T Instantiate<T>(params object[] args)
		{
			return instantiator.Instantiate<T>(args);
		}
	}
}