using System;
using System.Collections.Generic;
using Audio;
using Cysharp.Threading.Tasks;
using Events;
using RR.Core.ResourceManagament;
using RR.Core.Components;
using RR.Core.DebugSystem;
using UnityEngine;

namespace Vulcan.Audio
{
	public sealed class MusicController : Singleton<MusicController>
	{
		[SerializeField] private List<AudioSource> sources;
		private int currentIndex;

		protected override void OnAwake()
		{
			DataBus.AppData.Subscribe(this, appData => UpdateVolume(appData.MusicVolume));
			UpdateVolume(DataBus.AppData.Value.MusicVolume);
		}

		public static bool TryGetClip(string id, out Clip result)
		{
			result = Clip.None;
			return !string.IsNullOrEmpty(id) && Enum.TryParse(id, out result);
		}
		
		public static void TryPlay(string id)
		{
			if (string.IsNullOrEmpty(id) || !Enum.TryParse(id, out Clip result))
				return;
			
			Play(result);
		}

		public static void Play(Clip clip)
		{
			if (clip == Clip.None)
			{
				RRLogger.Error($"Cant play Clip.{clip} music");
				return;
			}

#if UNITY_EDITOR
			return;
#endif
			
			var source = Instance.GetFreeSource();
			source.LoadResourceAsync(clip.ToString())
				.ContinueWith(() => source.Play())
				.Forget();
		}
		
		private AudioSource GetFreeSource()
		{
			currentIndex = (currentIndex + 1) % sources.Count;
			var audioSource = sources[currentIndex];
			audioSource.volume = DataBus.AppData.Value.MusicVolume;
			return audioSource;
		}
		
		private void UpdateVolume(float volume)
		{
			foreach (var source in sources)
				source.volume = volume;
		}
	}
}
