using System;
using System.Threading.Tasks;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BerserkV3.Common.SceneService
{
	public class SceneService : ISceneService, IDisposable
	{
		public event Action<Scene> OnSceneLoaded;

		public Scene Current { get; private set; }

		public async UniTask LoadAsync(Scene scene)
		{
			RRLogger.Log($"Previous scene : {Current.ToString().Red()}");
			Current = scene;
			RRLogger.Log($"Scene loading : {scene.ToString().Green()}");
			var operation = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Single);
			while (!operation.isDone)
				await Task.Yield();
			
			OnSceneLoaded?.Invoke(scene);
		}

		public Progress<float> Load(Scene scene)
		{
			Current = scene;
			var progress = new Progress<float>();
			var operation = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Single);
			progress.ProgressTickAsync(operation, () => OnSceneLoaded?.Invoke(scene)).Forget();
			
			return progress;
		}

		public void Dispose()
		{
			OnSceneLoaded = null;
		}
	}
}