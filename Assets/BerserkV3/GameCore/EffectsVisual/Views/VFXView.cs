using System;
using System.Threading;
using BerserkV3.Common.AudioSystem;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual
{
	[RequireComponent(typeof(ParticleSystem))]
	public class VFXView : MonoBehaviour
	{
		[SerializeField] protected float particlesDelayS = 1f;
		[SerializeField] protected float disposeAfterSoftStopDelayS = 5f;
		[SerializeField] private Clip audioKeyWord;
		[SerializeField] protected bool attachToSource;
		[SerializeField] protected ParticleSystem particleSystem;
		[SerializeField] protected float forceDestroyDelayS = -1;
		[SerializeField] protected bool destroyAfterParticlesEnd;
		[SerializeField] protected bool onTop = false;
		[SerializeField] protected bool sortingsLateUpdate = false;
		[SerializeField] protected Vector2Int sortingOrders = new Vector2Int(-3, 3);

		private CancellationTokenSource lifetimeTokenSource;
		private UniTask animationTask;
		private bool defaultLoop;

		public bool AttachToSource => attachToSource;
		
		public Clip AudioKeyword => audioKeyWord;

		protected virtual void Awake()
		{
			lifetimeTokenSource = new CancellationTokenSource();
			if (particleSystem != null
			    || TryGetComponent(out particleSystem))
			{
				var main = particleSystem.main;
				main.stopAction = ParticleSystemStopAction.Callback;
				defaultLoop = main.loop;
			}

			SetupDetph();

			if (forceDestroyDelayS > 0 && lifetimeTokenSource != null)
				UniTask.Delay(TimeSpan.FromSeconds(forceDestroyDelayS), cancellationToken: lifetimeTokenSource.Token).ContinueWith(DestroyInstance);
		}

		protected virtual void LateUpdate()
		{
			if (sortingsLateUpdate)
				SetupDetph();
		}

		private ParticleSystemRenderer[] renderers;
		public virtual void SetupDetph()
		{
			renderers ??= GetComponentsInChildren<ParticleSystemRenderer>(true);
			var order = onTop ? sortingOrders.y : sortingOrders.x;
			foreach (var particle in renderers)
				particle.sortingOrder = order;
		}

		public virtual void SetArguments(params object[] args)
		{
		}

		public VFXView SetParent(Transform parent)
		{
			transform.SetParent(parent, false);
			return this;
		}

		public VFXView SetRotation(Quaternion rotation)
		{
			transform.rotation = rotation;
			return this;
		}

		public VFXView SetScale(Vector3 value)
		{
			transform.localScale = value;
			return this;
		}

		public VFXView SetLocalRotation(Quaternion rotation)
		{
			transform.localRotation = rotation;
			return this;
		}

		public VFXView SetPosition(Vector3 position)
		{
			transform.position = position;
			return this;
		}

		public VFXView SetLocalPosition(Vector3 localPosition)
		{
			transform.localPosition = localPosition;
			return this;
		}
		
		public VFXView SetParticlesLoop(bool isLoop)
		{
			if (particleSystem.Value() == null)
				return this;

			var main = particleSystem.main;
			main.loop = isLoop;
			return this;
		}

		public VFXView ForceStopParticles()
		{
			particleSystem.Value()?.Stop(true);
			return this;
		}

		public UniTask WaitForParticlesAsync()
		{
			if (particleSystem == null || particleSystem.isStopped)
				return UniTask.CompletedTask;

			return UniTask.Delay(TimeSpan.FromSeconds(particlesDelayS));
		}

		public VFXView SoftStop()
		{
			if (lifetimeTokenSource == null)
				return this;
			
			SetParticlesLoop(false);
			UniTask
				.Delay(TimeSpan.FromSeconds(disposeAfterSoftStopDelayS),cancellationToken:lifetimeTokenSource.Token)
				.ContinueWith(DestroyInstance)
				.Forget();
			return this;
		}

		public void DestroyInstance()
		{
			if (lifetimeTokenSource == null)
				return;
			
			var selfMonoInstance = gameObject.Value();
			if(!selfMonoInstance)
				return;
			
			SetParticlesLoop(defaultLoop);
			ForceStopParticles();
			Destroy(gameObject);
		}

		private void OnParticleSystemStopped()
		{
			if (destroyAfterParticlesEnd)
				DestroyInstance();
		}

		protected virtual void OnDestroy()
		{
			lifetimeTokenSource?.Cancel();
			lifetimeTokenSource?.Dispose();
			lifetimeTokenSource = null;
			renderers = null;
		}
	}
}