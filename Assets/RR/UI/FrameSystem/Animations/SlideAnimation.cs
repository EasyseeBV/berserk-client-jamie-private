using System;
using UnityEngine;

namespace RR.UI.FrameSystem.Animations
{
	[Serializable]
	public class SlideAnimation : BaseAnimation, ISerializationCallbackReceiver
	{
		public SlideDirection Direction = SlideAnimation.SlideDirection.RightIn;

		public SlideAnimation()
		{
			Curve.AddKey(new Keyframe(0, 0, 0, 0));
			Curve.AddKey(new Keyframe(1, 1, 0, 0));
		}

		[Serializable]
		public enum SlideDirection
		{
			RightIn, LeftIn, UpIn, DownIn,
			RightOut, LeftOut, UpOut, DownOut
		}

		float padLeft;
		float padRight;
		float padTop;
		float padBottom;

		public override void Init(RectTransform tr)
		{
			base.Init(tr);

			padLeft = (tr.localPosition.x + tr.rect.xMax * tr.localScale.x) - rect.xMin;
			padRight = rect.xMax - (tr.localPosition.x + tr.rect.xMin * tr.localScale.x);

			padTop = rect.yMax - (tr.localPosition.y + tr.rect.yMin * tr.localScale.y);
			padBottom = (tr.localPosition.y + tr.rect.yMax * tr.localScale.y) - rect.yMin;
		}

		public override void Apply(RectTransform tr, float time)
		{
			var pos = tr.localPosition;
			var val = Curve.Evaluate(time);
            
			switch (Direction)
			{
				case SlideDirection.LeftOut: pos = new Vector3(pos.x - val * padLeft, pos.y, pos.z); break;
				case SlideDirection.RightOut: pos = new Vector3(pos.x + val * padRight, pos.y, pos.z); break;
				case SlideDirection.DownOut: pos = new Vector3(pos.x, pos.y - val * padBottom, pos.z); break;
				case SlideDirection.UpOut: pos = new Vector3(pos.x, pos.y + val * padTop, pos.z); break;

				case SlideDirection.RightIn: val = 1 - val; goto case SlideDirection.LeftOut;
				case SlideDirection.LeftIn: val = 1 - val; goto case SlideDirection.RightOut;
				case SlideDirection.UpIn: val = 1 - val; goto case SlideDirection.DownOut;
				case SlideDirection.DownIn: val = 1 - val; goto case SlideDirection.UpOut;
			}

			tr.localPosition = pos;
		}
	}
}