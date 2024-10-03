using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.SceneService
{
	public interface ISceneService
	{
		event Action<Scene> OnSceneLoaded;
		Scene Current { get; }
		UniTask LoadAsync(Scene scene);
		Progress<float> Load(Scene scene);
	}
}