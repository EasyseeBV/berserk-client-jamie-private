using UnityEngine;

namespace BerserkV3.Common.AudioSystem.Abstractions
{
	public interface IAudioPlayer
	{
		float Volume { get; }

		void SetVolume(float value01);
		void Play(AudioClip clip, bool loop);
		void Stop();
	}
}