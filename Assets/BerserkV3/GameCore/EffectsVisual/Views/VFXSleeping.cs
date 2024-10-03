using System.Collections;
using DG.Tweening;
using Lean.Pool;
using TMPro;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual
{
	public class VFXSleeping : VFXView
	{
		[SerializeField] private TextMeshProUGUI particlePrefab;
		
		private void OnEnable()
			=> StartCoroutine(AnimationRoutine());

		private IEnumerator AnimationRoutine()
		{
			var yield = new WaitForSeconds(1);

			SpawnParticle();

			while (gameObject.activeSelf)
			{
				yield return yield;
				SpawnParticle();
			}
		}

		private void SpawnParticle()
		{
			var particle = LeanPool.Spawn(particlePrefab, transform);	
			Init(particle);
			GetAnimationSequence(particle).Play();
		}

		private void Init(TextMeshProUGUI particle)
		{
			particle.SetText("Z");
			particle.color = Color.white;
			particle.transform.localPosition = Vector2.zero;
		}

		private Sequence GetAnimationSequence(TextMeshProUGUI particle)
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