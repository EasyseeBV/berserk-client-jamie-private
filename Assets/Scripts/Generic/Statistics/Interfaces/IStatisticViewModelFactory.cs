using UnityEngine;

namespace Statistics
{
	public interface IStatisticViewModelFactory
	{
		IStatisticView Create(string id, Transform parent = null, int displayCount = 0);
	}
}