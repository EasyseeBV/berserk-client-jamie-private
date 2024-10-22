using BerserkV3.Common.AudioSystem.Abstractions;
using UnityEngine;
using Zenject;

namespace BerserkV3.Common.AudioSystem
{
	public class SimpleAudioPlayer : IAudioPlayer
	{
		private readonly AudioSource source;

		public float Volume { get; private set; }
		
		public SimpleAudioPlayer(IInstantiator instantiator)
		{
			source = instantiator.InstantiateComponentOnNewGameObject<AudioSource>(nameof(SimpleAudioPlayer));
			source.rolloffMode = AudioRolloffMode.Logarithmic;
			source.spatialBlend = 0f;
			source.maxDistance = 500f;
			source.minDistance = 1;
		}

		public void SetVolume(float value01)
		{
			Volume = value01;
			source.volume = value01;
		}

		public void Play(AudioClip clip, bool loop)
		{
			if (!source)
				return;

			if (!loop)
			{
				source.PlayOneShot(clip);
				return;
			}

			source.clip = clip;
			source.loop = true;
			source.Play();
		}

		public void Stop()
		{
			if (!source)
				return;
			
			source.Stop();
			source.clip = null;
		}
	}
}