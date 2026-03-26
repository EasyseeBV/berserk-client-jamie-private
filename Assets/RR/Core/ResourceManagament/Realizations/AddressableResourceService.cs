using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RR.Core.Utilities.DiscSpaceUtils;
using Object = UnityEngine.Object;
using UnityEngine;

#if ADDRESSABLES_INCLUDED
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RR.Core.ResourceManagament
{
	// See an expression in the asmdef to enable this feature
	// https://forum.unity.com/threads/detect-if-a-package-is-installed.1100338/
#if ADDRESSABLES_INCLUDED
	public class AddressableResourceService : IResourceService
	{
		private readonly string dataPath;
		private bool initialized;

		public AddressableResourceService(string dataPath)
		{
			this.dataPath = dataPath;
		}
		
		public async Task InitializeAsync(IProgress<float> progress = null)
		{
			progress ??= new ProgressMock();
			if (initialized)
			{
				progress.OnSkip();
				return;
			}
			
			try
			{
				ClearCacheCatalog();
				await Addressables.InitializeAsync(true);

				var updatableCatalogs = await Addressables.CheckForCatalogUpdates();
				if (updatableCatalogs is {Count: > 0})
				{
					var locators = await Addressables.UpdateCatalogs(updatableCatalogs.ToArray());
					await UpdateInternalCatalogsAsync(locators, progress); // updating
				}
				initialized = true;
			}
			catch (Exception)
			{
				initialized = false;
				throw;
			}
			finally
			{
				progress.OnSkip();
			}
		}

		public async Task<T> GetAsync<T>(string id, CancellationToken token = default) where T : Object
		{
			if (!initialized)
				throw new NotInitializedException();
			
			if (string.IsNullOrEmpty(id))
				throw new NullReferenceException($"Resource Id must be not null and empty, requested resource type : {nameof(T)}");

			ThrowIfCancelled(token);
			var handle = Addressables.LoadAssetAsync<T>(id);

			try
			{
				while (!handle.IsDone)
				{
					ThrowIfCancelled(token);
					await Task.Yield();
				}
				
				ThrowIfCancelled(token);
				if (handle.OperationException != null || !handle.Result)
				{
#if UNITY_EDITOR
					var editorAsset = LoadFromEditorAssetDatabase<T>(id);
					if (editorAsset)
						return editorAsset;
#endif
					if (handle.OperationException != null)
						throw new Exception($"Resource not loaded for id : {id}, " +
						                    $"requested resource type : {nameof(T)}, " +
						                    $"original Exception : {handle.OperationException}");

					throw new ApplicationException($"Internal error resource not loaded for id : {id}, " +
					                               $"requested resource type : {nameof(T)}");
				}
				
				return handle.Result;
			}
			catch (OperationCanceledException)
			{
				Addressables.Release(handle); // do not use in finally, it can release resource when errors

				// Override the exception
				throw new OperationCanceledException($"Task was cancelled, resourceId : {id}, " +
				                                     $"requested resource type {typeof(T).Name}");
			}
			catch (Exception)
			{
				Addressables.Release(handle); // do not use in finally, it can release resource when errors
				throw;
			}
		}

		public void Release(params object[] values)
		{
			foreach (var obj in values)
			{
				Addressables.Release(obj);
			}
		}
		
		private async Task HandleProgress(IProgress<float> progress, ICollection<AsyncOperationHandle> handlers)
		{
			long downloaded = 0;
			var totalDownloadBytes = handlers.Sum(x=> x.GetDownloadStatus().TotalBytes);

			progress.OnStarted();
			while (downloaded < totalDownloadBytes)
			{
				ThrowIfCancelled();
				if (handlers.Any(x => x.Status == AsyncOperationStatus.Failed))
					throw new InternetConnectionException("Code : 400, Can't download, process failed.");
				
				downloaded = handlers.Sum(x => x.GetDownloadStatus().DownloadedBytes);
				progress.Report((float)downloaded / totalDownloadBytes);
				await RefrehTaskAsync();
			}
			progress.OnEnded();
		}

		private async Task UpdateInternalCatalogsAsync(
			IList<IResourceLocator> locators, 
			IProgress<float> progress)
		{
			if (locators.Count == 0)
			{
				progress.OnSkip();
				return;
			}

			var downloadSizeOperations = new List<AsyncOperationHandle<long>>();
			var downloadOperations = new List<AsyncOperationHandle>();

			try
			{
				downloadSizeOperations.AddRange(locators.Select(x => Addressables.GetDownloadSizeAsync(x.Keys)));
				var downloadSizeBytesList = await Task.WhenAll(downloadSizeOperations.Select(x => x.Task));
				var totalSpaceToDownload = downloadSizeBytesList.Sum();
				
				if (totalSpaceToDownload <= 0)
				{
					progress.OnSkip();
					return;
				}
				
				DiscSpaceChecker.ThrowWhenNotEnoughMemoryFor(totalSpaceToDownload, dataPath);

				for (var i = 0; i < locators.Count; i++)
				{
					if (downloadSizeBytesList[i] <= 0)
						continue;

					// download and make new cache by keys
					downloadOperations.AddRange(locators[i].Keys
						.Select(key => Addressables.DownloadDependenciesAsync(key)));
				}

				if (downloadOperations.Count == 0)
				{
					progress.OnSkip();
					return;
				}

				await HandleProgress(progress, downloadOperations);
			}
			finally
			{
				downloadOperations.ForEach(Addressables.Release);
				downloadSizeOperations.ForEach(Addressables.Release);
				downloadSizeOperations.Clear();
				downloadOperations.Clear();
			}
		}

		private Task RefrehTaskAsync(CancellationToken token = default)
		{
			return Task.Delay(1000, token); // refresh operations 1 seconds
		}

		private void ClearCacheCatalog()
		{
			var catalogPath = $"{dataPath}/com.unity.addressables";
			if(Directory.Exists(catalogPath))
			   Directory.Delete(catalogPath,true);
		}

#if UNITY_EDITOR && ADDRESSABLES_INCLUDED
		private static T LoadFromEditorAssetDatabase<T>(string id) where T : Object
		{
			var guids = AssetDatabase.FindAssets($"{id} t:{typeof(T).Name}", new[] {"Assets"});
			foreach (var guid in guids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(assetPath))
					continue;

				var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if (asset)
					return asset;
			}

			// Some addressable entries are textures imported as sprites, so
			// this fallback keeps editor-local play mode usable for both.
			if (typeof(T) == typeof(Texture) || typeof(T) == typeof(Texture2D))
			{
				var guidsByName = AssetDatabase.FindAssets(id, new[] {"Assets"});
				foreach (var guid in guidsByName)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(guid);
					if (string.IsNullOrEmpty(assetPath))
						continue;

					var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (texture)
						return texture as T;
				}
			}

			return null;
		}
#endif
		
		public static void ThrowIfCancelled(CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();
			if (!Application.isPlaying)
				throw new OperationCanceledException();
		}
	}
	#endif
}
