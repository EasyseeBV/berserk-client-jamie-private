using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statistics
{
	public interface IStatisticsRepository
	{
		public void Add(params StatisticModel[] models);

		public void Remove(string id);

		public StatisticModel Get(string id);

		public bool Contains(string id);
		
		public void Clear();
	}
}