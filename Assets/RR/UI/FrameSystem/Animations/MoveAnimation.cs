using System;
using UnityEngine;

namespace RR.UI.FrameSystem.Animations
{
	[Serializable]
	public class MoveAnimation : BaseAnimation
	{
		Vector2 from;
		Vector2 to;

		public MoveAnimation(Vector2 from, Vector2 to)
		{
			this.from = from;
			this.to = to;

			Curve.AddKey(0, 0);
			Curve.AddKey(0.25f, 0.1f);
			Curve.AddKey(0.75f, -0.1f);
			Curve.AddKey(1, 0f);
		}

		public override void Apply(RectTransform tr, float time)
		{
			var val = Curve.Evaluate(time);
			tr.localPosition = Vector2.Lerp(from, to, val);
		}
	}
}