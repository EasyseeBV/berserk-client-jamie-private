using DG.Tweening;
using Lean.Pool;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Vulcan.VFX.Custom
{
	public abstract class TextEffect : MonoBehaviour
	{
		[SerializeField] private TextMeshPro particlePrefab = default;
		[SerializeField] private bool isOneShot = default;

		[SerializeField] protected float scale = default;

		private void OnEnable()
			=> StartCoroutine(AnimationRoutine());

		public IEnumerator AnimationRoutine()
		{
			var yield = new WaitForSeconds(1);

			SpawnParticle();
			if (isOneShot) yield break;

			while (gameObject.activeSelf)
			{
				yield return yield;
				SpawnParticle();
			}
		}

		private void SpawnParticle()
		{
			var particle = LeanPool.Spawn(particlePrefab, transform);	
			InitializeText(particle);
			GetAnimationSequence(particle).Play();
		}

		protected abstract void InitializeText(TextMeshPro particle);

		protected abstract Sequence GetAnimationSequence(TextMeshPro particle);
	}
}