using UnityEngine;

namespace BerserkV3.Common.InputSystem.HoveringSystem
{
	public class HoveringSettings : IHoverableSetting
	{
		public Vector3 DefaultSize { get; set; }
		public float MaxSize { get; set; }
		public float Duration { get; set; }
		public int MaxSublingIndex { get; set; }
		public bool ChangeSublingIndex { get; set; }
		public bool UseScaleUpAnimation { get; set; }
		public bool Enabled { get; set; }

		public HoveringSettings()
		{
			ApplyDefaultValues();
		}

		public static HoveringSettings Default()
		{
			return new HoveringSettings().ApplyDefaultValues();
		}

		private HoveringSettings ApplyDefaultValues()
		{

			DefaultSize = Vector3.one;
			MaxSize = 1.2f;
			Duration = 0.25f;
			MaxSublingIndex = 1000;
			ChangeSublingIndex = true;
			UseScaleUpAnimation = true;
			Enabled = true;
			return this;
		}
	}
}