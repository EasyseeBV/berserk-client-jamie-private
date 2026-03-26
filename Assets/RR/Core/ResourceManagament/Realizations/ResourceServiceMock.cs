using System;
using System.Threading;
using System.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RR.Core.ResourceManagament
{
	public class ResourceServiceMock : IResourceService
	{
		private void LogUsingMock()
		{
			RRLogger.Log("You trying to use " + $"{nameof(ResourceServiceMock).Red()}");
		}

		public Task InitializeAsync(IProgress<float> progress = null)
		{
			LogUsingMock();
			return Task.CompletedTask;
		}

		public Task<T> GetAsync<T>(string id, CancellationToken token = default) where T : Object
		{
			LogUsingMock();
			if (string.IsNullOrWhiteSpace(id))
				return Task.FromResult<T>(null);

#if UNITY_EDITOR
			return Task.FromResult(LoadFromEditorAssetDatabase<T>(id));
#else
			return Task.FromResult<T>(null);
#endif
		}

		public void Release(params object[] values)
		{
			LogUsingMock();
		}

#if UNITY_EDITOR
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

			if (typeof(T) != typeof(UnityEngine.Texture) && typeof(T) != typeof(UnityEngine.Texture2D))
				return null;

			var fallbackGuids = AssetDatabase.FindAssets(id, new[] {"Assets"});
			foreach (var guid in fallbackGuids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(assetPath))
					continue;

				var texture = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(assetPath);
				if (texture)
					return texture as T;
			}

			return null;
		}
#endif
	}
}
