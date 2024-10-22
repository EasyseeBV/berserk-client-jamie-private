using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BerserkV3.Common.AudioSystem
{
	[CreateAssetMenu(menuName = "SimpleMaps/AudioReferences", fileName = "AudioReference")]
	public class AudioReferences : ScriptableObject
	{
		[Serializable]
		private class AudioReference
		{
			public Clip Key;
			public AssetReferenceT<AudioClip> Reference;
		}

		[SerializeField] private List<AudioReference> clipMap = new();

		// TODO it's 
		public async UniTask<AudioClip> GetAudioClip(Clip key)
		{
			var asset = clipMap.FirstOrDefault(x => x.Key == key);
			if (asset != null)
			{
				var handle = asset.Reference.LoadAssetAsync<AudioClip>();
				return await handle.Task;
			}

			return default;
		}

		[ContextMenu("Init Map")]
		private void Init()
		{
			clipMap ??= new List<AudioReference>();
			clipMap.Clear();
			foreach (Clip clip in Enum.GetValues(typeof(Clip)))
			{
				clipMap.Add(new AudioReference {Key = clip});
			}
		}
	}
}