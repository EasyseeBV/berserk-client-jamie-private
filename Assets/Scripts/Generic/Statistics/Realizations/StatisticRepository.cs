using System;
using System.Collections.Generic;
using RR.Core.DebugSystem;

namespace Statistics
{
	public class StatisticRepository : IStatisticsRepository
	{
		private readonly Dictionary<string, StatisticModel> storage;

		public StatisticRepository()
		{
			storage = new Dictionary<string, StatisticModel>();
		}

		public void Add(params StatisticModel[] models)
		{
			foreach (var model in models)
			{
				if (Contains(model.Id))
					throw new InvalidOperationException($"Model with id : {model.Id}, alredy exist in collection");
				
				storage.Add(model.Id, model);
			}
		}

		public void Remove(string id)
		{
			if (!storage.Remove(id))
				RRLogger.Warning($"You try remove items with id :{id} but id does not represented in collection");
		}

		public StatisticModel Get(string id)
		{
			return Contains(id) 
				? storage[id] 
				: throw new InvalidOperationException($"Model with id : {id}, not represented in collection");
		}

		public bool Contains(string id)
		{
			return storage.ContainsKey(id);
		}

		public void Clear()
		{
			storage.Clear();
		}
	}
}