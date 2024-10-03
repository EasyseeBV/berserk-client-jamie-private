using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using TMPro;

namespace Vulcan.VFX.Custom
{
	public class ImmuneEffect : TextEffect
	{
		protected override void InitializeText(TextMeshPro particle)
		{
			particle.SetText("IMMUNE");
			particle.color = Color.yellow;
			
			particle.transform.localScale = Vector3.one * scale;
			particle.transform.localPosition = Vector3.forward * 100;
			particle.transform.rotation = Quaternion.Euler(90, 0, 0);
		}

		protected override Sequence GetAnimationSequence(TextMeshPro particle)
		{
			return DOTween.Sequence()
				.Join(particle.DOFade(1, 0.53f).From(0))
				.Join(particle.transform.DOLocalMoveY(-280, 3f))
				.OnComplete(() => LeanPool.Despawn(particle));
		}
	}
}