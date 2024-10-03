using System;
using System.Threading.Tasks;

namespace Statistics
{
	public interface IStatisticApplication : IDisposable
	{
		Task<bool> Init();
		
		bool AvailableStatistic(string id);
	}
}