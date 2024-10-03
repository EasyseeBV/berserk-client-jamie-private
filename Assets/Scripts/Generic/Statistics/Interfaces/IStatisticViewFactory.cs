using RR.UI.FrameSystem;
using UnityEngine;

namespace Statistics
{
	public interface IStatisticViewFactory
	{
		BaseView Create(string id, Transform parent = null);
	}
}