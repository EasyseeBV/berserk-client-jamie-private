using Vulcan.Audio;
using UnityEngine;
using Lean.Pool;
using System;
using System.Collections;
using System.Linq;
using Audio;
using RR.Core.DebugSystem;

namespace Vulcan.VFX
{
	public class VFXEntity : MonoBehaviour
	{
		public VFXKey Key = default;
		public bool AttachToSource => attachToSource;
		public bool IsOneShot => isOneShot;

		[SerializeField] private Clip audioClip = default;
		[SerializeField] private bool attachToSource = false;
		[SerializeField] private bool isOneShot = true;
		[SerializeField] private bool isInFront = true;
		[SerializeField] protected float lifeTime = 0.5f;

		private bool initialized;
		private float despawnTime;
		private Coroutine lifeTimeProcess;
		private Func<bool> shouldDespawn;
		private const float LOCAL_OFFSET = 60;
		private const float GLOBAL_OFFSET = 1;

		public virtual VFXEntity Initialize()
		{
			if (initialized)
				return this;


			initialized = true;
			DisableBrokenRenderers();

			if (TryGetComponent<ParticleSystem>(out var particle))
			{
				lifeTime = particle.main.duration;	
			}

			transform.localRotation = Quaternion.identity;

			return this;
		}

		private void DisableBrokenRenderers()
		{
			foreach (var renderer in GetComponentsInChildren<ParticleSystemRenderer>(true))
			{
				if (!renderer)
					continue;

				var materials = renderer.sharedMaterials;
				if (materials == null || materials.Length == 0)
					continue;

				var hasBrokenMaterial = materials.Any(material =>
					material == null
					|| material.shader == null
					|| material.shader.name == "Hidden/InternalErrorShader"
					|| !material.shader.isSupported);

				if (!hasBrokenMaterial)
					continue;

				renderer.enabled = false;
				RRLogger.Warning($"Disabled broken legacy VFX renderer on {name}");
			}
		}

		public VFXEntity SetPosition(Vector3 position)
		{
			transform.position = SetOffset(position,true);
			return this;
		}

		public VFXEntity SetLocalPosition(Vector3 localPosition)
		{
			transform.localPosition = SetOffset(localPosition,false);
			return this;
		}

		public virtual VFXEntity SetTargetPosition(Vector3 targetPosition) => this;

		public virtual void Play(Func<bool> shouldDespawn = null)
		{
			this.shouldDespawn = shouldDespawn;
			despawnTime = Time.fixedTime + lifeTime;
			AudioController.Play(audioClip);
			
			if (lifeTimeProcess != null)
			{
				StopCoroutine(lifeTimeProcess);
			}
			lifeTimeProcess = StartCoroutine(LifeTimeTimer());
		}

		protected bool OnComplete()
		{
			if (Time.fixedTime <= despawnTime)
			{
				return false;
			}
			
			if (isOneShot || shouldDespawn == null || shouldDespawn.Invoke())
			{
				LeanPool.Despawn(this);
				shouldDespawn = null;
				despawnTime = 0;
				return true;
			}

			return false;
		}

		private IEnumerator LifeTimeTimer()
		{
			while (!OnComplete())
			{
				yield return null;
			}

			lifeTimeProcess = null;
		}

		protected Vector3 SetOffset(Vector3 target, bool worldOffset)
		{
			return target 
			       + (worldOffset ? Vector3.up : Vector3.back) 
			       * ((isInFront ? 1 : -1) * (worldOffset ? GLOBAL_OFFSET : LOCAL_OFFSET));

		}
	}
}
