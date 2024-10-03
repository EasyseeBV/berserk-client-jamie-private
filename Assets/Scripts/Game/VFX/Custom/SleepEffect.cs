using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using TMPro;

namespace Vulcan.VFX.Custom
{
	public class SleepEffect : TextEffect
	{
		protected override void InitializeText(TextMeshPro particle)
		{
			particle.SetText("Z");
			particle.color = Color.white;
			particle.transform.localPosition = Vector2.zero;
		}

		protected override Sequence GetAnimationSequence(TextMeshPro particle)
		{
			return DOTween.Sequence()
				.Join(particle.transform.DOLocalPath(new[] { new Vector3(35, 40), new Vector3(60, 55), new Vector3(40, 135), }, 1.65f, PathType.CubicBezier, PathMode.Sidescroller2D))
				.Join(particle.DOFade(1, 0.9f).From(0))
				.Join(particle.DOFade(0, 0.75f).SetDelay(0.75f))
				.Join(particle.DOScale(1.05f, 0.75f).From(0.8f))
				.Join(particle.DOScale(0.65f, 0.75f).SetDelay(0.75f))
				.OnComplete(() => LeanPool.Despawn(particle));
		}
	}
}