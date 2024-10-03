using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RR.UI.FrameSystem.Animations
{
	public interface IAnimation
	{
		IEnumerator Play(RectTransform rt, float timeScale = 1, int repeatCount = 1);
		void StopAndResetTransform();
	}

	[Serializable]
	public class Animation : IAnimation
	{
		public float TimeScale = 3;
		public int RepeatCount = 1;

		[HideInInspector]
		public List<SingleAnimation> Sequence = new List<SingleAnimation>();

		IEnumerator IAnimation.Play(RectTransform rt, float timeScale, int repeatCount)
		{
			if (repeatCount < 0)
				repeatCount = int.MaxValue;

			repeatCount *= RepeatCount;

			for (int i = 0; i < repeatCount; i++)
				foreach (var a in Sequence.Cast<IAnimation>())
					yield return a.Play(rt, TimeScale * timeScale);
		}

		void IAnimation.StopAndResetTransform()
		{
			foreach (var a in Sequence.Cast<IAnimation>())
				a.StopAndResetTransform();
		}

		public IAnimation CreateInstance()
		{
			var res = (Animation)MemberwiseClone();
			res.Sequence = Sequence.Select(a => (SingleAnimation)a.CreateInstance()).ToList();
			return res;
		}
	}

	[Serializable]
	public class SingleAnimation : IAnimation
	{
		private static readonly int MaxAnimationTime = 1;
		public float TimeScale = 1;
		public int RepeatCount = 1;

		[HideInInspector]
		public List<BaseAnimation> Animations = new List<BaseAnimation>();

		[NonSerialized]
		TransformInfo source;
		[NonSerialized]
		RectTransform rt;
		[NonSerialized]
		bool stop;

		IEnumerator IAnimation.Play(RectTransform rt, float timeScale, int repeatCount)
		{
			stop = false;
			source = new TransformInfo(rt);
			this.rt = rt;
			timeScale *= TimeScale;
			repeatCount *= RepeatCount;

			foreach (var a in Animations)
				a.Init(rt);

			for (int i = 0; i < repeatCount; i++)
			{
				var time = 0f;
				while (time < MaxAnimationTime)
				{
					if (stop)
						yield break;

					foreach (var a in Animations)
						a.Apply(rt, time);

					yield return null;

					time += Time.unscaledDeltaTime * timeScale;
					source.Apply(rt);
				}
				
				foreach (var a in Animations)
					a.Apply(rt, MaxAnimationTime);
				source.Apply(rt);
			}

			foreach (var a in Animations)
				a.OnFinish();
		}

		void IAnimation.StopAndResetTransform()
		{
			foreach (var a in Animations)
				a.OnFinish();

			source?.Apply(rt);
			stop = true;
		}

		public IAnimation CreateInstance()
		{
			var res = (SingleAnimation)MemberwiseClone();
			res.Animations = Animations.Select(a => a.Clone()).ToList();
			return res;
		}

		class TransformInfo
		{
			Vector3 localPosition;
			Vector3 localScale;
			Quaternion localRotation;

			public TransformInfo(RectTransform tr)
			{
				localPosition = tr.localPosition;
				localScale = tr.localScale;
				localRotation = tr.localRotation;
			}

			public void Apply(RectTransform tr)
			{
				if (!tr)
					return;

				tr.localPosition = localPosition;
				tr.localScale = localScale;
				tr.localRotation = localRotation;
			}
		}
	}

	[Serializable]
	public abstract class BaseAnimation
	{
		[NonSerialized]
		public AnimationCurve Curve = new AnimationCurve();

		[NonSerialized]
		protected Rect rect;

		byte[] raw;

		public void OnAfterDeserialize()
		{
			Curve = new AnimationCurve();

			if (raw != null)
				using (var ms = new MemoryStream(raw))
				using (var bw = new BinaryReader(ms))
				{
					var count = bw.ReadInt32();
					for (int i = 0; i < count; i++)
					{
						Curve.AddKey(new Keyframe
						{
							time = bw.ReadSingle(),
							value = bw.ReadSingle(),
							inTangent = bw.ReadSingle(),
							outTangent = bw.ReadSingle(),
							inWeight = bw.ReadSingle(),
							outWeight = bw.ReadSingle(),
							weightedMode = (WeightedMode)bw.ReadByte()
						});
					}
				}

			raw = null;
		}

		public void OnBeforeSerialize()
		{
			using (var ms = new MemoryStream())
			using (var bw = new BinaryWriter(ms))
			{
				bw.Write(Curve.keys.Length);
				foreach (var kf in Curve.keys)
				{
					bw.Write(kf.time);
					bw.Write(kf.value);
					bw.Write(kf.inTangent);
					bw.Write(kf.outTangent);
					bw.Write(kf.inWeight);
					bw.Write(kf.outWeight);
					bw.Write((byte)kf.weightedMode);
				}

				raw = ms.ToArray();
			}
		}

		public virtual void Init(RectTransform tr)
		{
			rect = new Rect(0, 0, Screen.width, Screen.height);

			var parent = tr.parent as RectTransform;
			if (parent)
				rect = parent.rect;
		}

		public virtual void OnFinish()
		{
		}

		public BaseAnimation Clone()
		{
			return (BaseAnimation)MemberwiseClone();
		}

		public abstract void Apply(RectTransform tr, float time);
	}
}