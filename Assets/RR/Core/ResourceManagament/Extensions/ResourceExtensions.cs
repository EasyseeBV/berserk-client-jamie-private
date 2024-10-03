using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#if UNITASK_INCLUDED
using Cysharp.Threading.Tasks;
#endif

#if ADDRESSABLES_INCLUDED
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace RR.Core.ResourceManagament
{
	public static class ResourceExtensions
	{
		// See an expression in the asmdef to enable this feature
		// https://forum.unity.com/threads/detect-if-a-package-is-installed.1100338/
#if UNITASK_INCLUDED
		public static async UniTask LoadResourceAsync(
			this Object target,
			string resourceId,
			CancellationToken token = default,
			bool relesePrevious = true)
		{

			await LoadResourceInternalAsync(target, resourceId, relesePrevious, token);
		}
#else
		public static async Task LoadResourceAsync(
			this Object target,
			string resourceId,
			CancellationToken token = default,
			bool relesePrevious = true)
		{
			
			await LoadResourceInternalAsync(target, resourceId, relesePrevious, token);
		}
#endif

		public static void ReleaseResource(this Object target)
		{
			if (!target)
				return;

			Object resource;
			switch (target)
			{
				case RawImage rawImage : 
					resource = rawImage.texture;
					rawImage.texture = null;
					break;
				
				case Image image : 
					resource = image.sprite;
					image.sprite = null;
					break;
				
				case AudioSource audioSource : 
					resource = audioSource.clip;
					audioSource.clip = null;
					break;
				
				default: resource = target;
					break;
			}

			if (!resource)
				return;

			try
			{
				ResourceServiceAdapter.Instance.Release(resource);
			}
			catch (Exception e)
			{
				RRLogger.Warning(e.Message);
			}
		}
		
		// See an expression in the asmdef to enable this feature
		// https://forum.unity.com/threads/detect-if-a-package-is-installed.1100338/
#if ADDRESSABLES_INCLUDED
		public static TaskAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> handle)
		{
			return handle.Task.GetAwaiter();
		}
#endif
		
		private static async Task LoadResourceInternalAsync(
			this Object target,
			string resourceId,
			bool relesePrevious = true,
			CancellationToken token = default)
		{
			
			if (!target)
			{
				RRLogger.Log($"Missing {nameof(target).Red()} reference, resource id : {resourceId.Red()}");
				return;
			}
			
			if (string.IsNullOrEmpty(resourceId))
			{
				RRLogger.Error($"Missing or empty {nameof(resourceId).Red()}, for target : {target.name.Red()}");
				return;
			}
			
			Object resource = default;
			try
			{
				resource = await LoadResourceInternalAsync(target, resourceId, token);
				token.ThrowIfCancellationRequested();
		
				if (relesePrevious)
					target.ReleaseResource();
				
				SetResourceInternal(resource, target);
			}
			catch (OperationCanceledException e)
			{
				ReleaseResource(resource);
				RRLogger.Log(e.Message.Orange()); // it's not an error
				throw; // need rethrow to extern handle
			}
			catch (NullReferenceException e)
			{
				ReleaseResource(resource);
				RRLogger.Log(e.Message.Red()); // it's possible an error we log simple
				throw; // need rethrow to extern handle
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				ReleaseResource(resource);
				throw; // need rethrow to extern handle
			}
		}

		private static void SetResourceInternal(Object resource, Object target)
		{
			if (!target)
				throw new NullReferenceException("Target object is missing");
			
			if (!resource)
				throw new NullReferenceException($"Resource object is missing for : {target}");
			
			switch (resource, target)
			{
				case (Texture texture, RawImage rawImage) : 
					rawImage.texture = texture; 
					break;
				
				case (Sprite sprite, Image image) : 
					image.sprite = sprite; 
					break;
				
				case (AudioClip audioClip, AudioSource audioSource): 
					audioSource.clip = audioClip; 
					break;
				
				default: throw new NotImplementedException($"Can't set the resource : {resource.GetType().Name}, " +
				                                           $"Into target : {target.GetType().Name}");
			}
		}

		private static async Task<Object> LoadResourceInternalAsync(Object target, string id, CancellationToken token)
		{
			return target switch
			{
				RawImage => await ResourceServiceAdapter.Instance.GetAsync<Texture>(id, token),
				Image => await ResourceServiceAdapter.Instance.GetAsync<Sprite>(id, token),
				AudioSource => await ResourceServiceAdapter.Instance.GetAsync<AudioClip>(id, token),
				_ => throw new NotImplementedException($"Cant load resource for target type : {target.GetType().Name}")
			};
		}

		public static void OnStarted(this IProgress<float> progress)
		{
			if (progress is IContentUpdateProgress updateProgress)
				updateProgress.OnStarted();
		}
		
		public static void OnEnded(this IProgress<float> progress)
		{
			if (progress is IContentUpdateProgress updateProgress)
				updateProgress.OnEnded();
		}
		
		public static void OnSkip(this IProgress<float> progress)
		{
			if (progress is IContentUpdateProgress updateProgress)
			{
				updateProgress.OnSkip();
				return;
			}

			progress?.Report(1f);
		}
	}
}