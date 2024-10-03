using System;
using UnityEngine;

namespace Statistics
{
	public class StatisticViewModelFactory : IStatisticViewModelFactory
	{
		private readonly IStatisticViewFactory viewFactory;
		private readonly IStatisticsRepository repository;

		public StatisticViewModelFactory(IStatisticViewFactory viewFactory, 
		                                 IStatisticsRepository repository)
		{
			this.viewFactory = viewFactory;
			this.repository = repository;
		}

		public IStatisticView Create(string id, Transform parent = null, int displayCount = 0)
		{
			if (!repository.Contains(id))
				return null;
			
			var model = repository.Get(id);
			return id switch
			{
				"MMR" => new EloStatisticView(model, viewFactory, parent, displayCount),
				"FavoriteDeck" => new FavoriteDeckStatisticView(model, viewFactory, parent, displayCount),
				"FavoriteFaction" => new FavoriteFactionStatisticView(model, viewFactory, parent),
				"FavoriteVulcanite" => new FavoriteVulcaniteStatisticView(model, viewFactory, parent),
				"InGameStats" => new InGameStatisticView(model, viewFactory, parent, displayCount),
				"PlayerOne" => new VulcanitesWinRateStatisticView(model, viewFactory, parent, displayCount),
				"PlayerTwo" => new VulcanitesWinRateStatisticView(model, viewFactory, parent, displayCount),
				_ => throw new NotImplementedException($"ViewModel with id : '{model.Id}' does not implemented in factory")
			};
		}
	}
}