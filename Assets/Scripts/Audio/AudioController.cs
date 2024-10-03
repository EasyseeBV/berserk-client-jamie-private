using System.Collections.Generic;
using Audio;
using Cysharp.Threading.Tasks;
using Events;
using RR.Core.ResourceManagament;
using RR.Core.Components;
using UnityEngine;

namespace Vulcan.Audio
{
	public sealed class AudioController : Singleton<AudioController>
	{
		[SerializeField] private List<AudioSource> sources;
		private int currentIndex;
		
		protected override void OnAwake()
		{
			DataBus.AppData.Subscribe(this, appData => UpdateVolume(appData.AudioVolume));
			UpdateVolume(DataBus.AppData.Value.AudioVolume);
		}
		
		public static void Play(Clip clip)
		{
			if (clip == Clip.None)
				return;
			
			var source = Instance.GetFreeSource();
			source.LoadResourceAsync(clip.ToString())
				.ContinueWith(() => source.Play())
				.Forget();
		}

		public static void SetVolume(float value)
		{
			if (Instance)
				Instance.UpdateVolume(value);
		}

		private AudioSource GetFreeSource()
		{
			currentIndex = (currentIndex + 1) % sources.Count;
			var audioSource = sources[currentIndex];
			audioSource.volume = DataBus.AppData.Value.AudioVolume;
			return audioSource;
		}
		
		private void UpdateVolume(float volume)
		{
			foreach (var source in sources)
				source.volume = volume;
		}
	}
}