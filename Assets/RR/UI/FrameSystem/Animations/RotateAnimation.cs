using System;
using UnityEngine;

namespace RR.UI.FrameSystem.Animations
{
	[Serializable]
	public class RotateAnimation : BaseAnimation
	{
		public RotateAnimation()
		{
			Curve.AddKey(0, 0);
			Curve.AddKey(1, 1);
		}

		public override void Apply(RectTransform tr, float time)
		{
			var val = Curve.Evaluate(time);
			tr.Rotate(new Vector3(0, 0, val * 360), Space.Self);
		}
	}
}