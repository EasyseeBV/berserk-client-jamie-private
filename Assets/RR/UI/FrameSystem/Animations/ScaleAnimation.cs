using System;
using UnityEngine;

namespace RR.UI.FrameSystem.Animations
{
	[Serializable]
	public class ScaleAnimation : BaseAnimation
	{
		[SerializeField]
		public Axis ScaleAxis = ScaleAnimation.Axis.All;

		[SerializeField]
		public float Scale = 1;

		[SerializeField]
		public bool ScaleToParentSize = true;

		public ScaleAnimation()
		{
			Curve.AddKey(0, 0);
			Curve.AddKey(1, 1);
		}

		public enum Axis
		{
			All, X, Y
		}

		public override void Apply(RectTransform tr, float time)
		{
			var scale = tr.localScale;
			var val = Curve.Evaluate(time);
			val *= Scale;

			switch (ScaleAxis)
			{
				case Axis.All: tr.localScale = new Vector3(tr.localScale.x * val, tr.localScale.y * val, 1); break;
				case Axis.X: tr.localScale = new Vector3(tr.localScale.x * val, 1, 1); break;
				case Axis.Y: tr.localScale = new Vector3(1, tr.localScale.y * val, 1); break;
			}
		}
	}
}