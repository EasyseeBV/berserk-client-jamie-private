using UnityEngine;

namespace BerserkV3.Common.InputSystem.HoveringSystem
{
	public interface IHoverableSetting
	{
		Vector3 DefaultSize { get; set; }
		float MaxSize { get; set; }
		float Duration { get; set; }
		int MaxSublingIndex { get; set; }
		bool ChangeSublingIndex { get; set; }
		bool UseScaleUpAnimation { get; set; }
		bool Enabled { get; set; }
	}
}