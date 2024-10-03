using System;
using UnityEngine;

namespace RR.UI.FrameSystem.Animations
{
	[Serializable]
	public class OffsetAnimation : BaseAnimation
	{
		[SerializeField]
		public Axis OffsetAxis = OffsetAnimation.Axis.X;

		[SerializeField]
		public float Amplitude = 1;

		[SerializeField]
		public bool ScaleToParentSize = true;

		[Serializable]
		public enum Axis
		{
			X, Y
		}

		public OffsetAnimation()
		{
			Curve.AddKey(0, 0);
			Curve.AddKey(0.25f, 0.1f);
			Curve.AddKey(0.75f, -0.1f);
			Curve.AddKey(1, 0f);
		}

		public override void Apply(RectTransform tr, float time)
		{
			var pos = tr.localPosition;
			var val = Curve.Evaluate(time);

			val *= Amplitude;

			switch (OffsetAxis)
			{
				case Axis.X:
					if (ScaleToParentSize)
						val *= rect.width;
					pos = new Vector3(pos.x + val, pos.y, pos.z);
					break;
				case Axis.Y:
					if (ScaleToParentSize)
						val *= rect.height;
					pos = new Vector3(pos.x, pos.y + val, pos.z);
					break;
			}

			tr.localPosition = pos;
		}
	}
}