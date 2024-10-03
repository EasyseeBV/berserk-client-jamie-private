using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RR.UI.FrameSystem
{
	/// <summary>Provides view's background sounds</summary>
	public class SoundManager
	{
		public float FadeVolume = 0.25f;
		public float VolumeChangeSpeed = 1f;
		private Dictionary<AudioClip, ViewSoundInfo> viewSounds = new Dictionary<AudioClip, ViewSoundInfo>();
		private AudioSource backgroundSoundPrefab;
		private MonoBehaviour owner;

		class ViewSoundInfo
		{
			public AudioSource AudioSource;
			public int ViewsCounter;
			public int FadeViewsCounter;
			public int MuteViewsCounter;
			public float InitVolume;
			public Coroutine Coroutine;
		}

		public SoundManager(MonoBehaviour owner, AudioSource backgroundSoundPrefab, float fadeVolume, float volumeChangeSpeed)
		{
			this.owner = owner;
			this.backgroundSoundPrefab = backgroundSoundPrefab;
			this.FadeVolume = fadeVolume;
			this.VolumeChangeSpeed = volumeChangeSpeed;
		}

		public void OnViewClosed(BaseView view)
		{
			if (view.Sounds.CloseSound != null)
				UIManager.PlayOneShotSound(view.Sounds.CloseSound);

			if (view.Sounds.BackgroundSound != null)
				StopPlay(view.Sounds.BackgroundSound);

			if (view.Sounds.OwnerSoundMode == OwnerSoundMode.Normal)
				return;

			var owner = view.Owner;
			while (owner != null)
			{
				if (owner.Sounds.BackgroundSound != null)
					switch (view.Sounds.OwnerSoundMode)
					{
						case OwnerSoundMode.Fade: StopFade(owner.Sounds.BackgroundSound); break;
						case OwnerSoundMode.Mute: StopMute(owner.Sounds.BackgroundSound); break;
					}
				owner = owner.Owner;
			}
		}

		public void OnViewShown(BaseView view)
		{
			if (view.Sounds.ShowSound != null)
				UIManager.PlayOneShotSound(view.Sounds.ShowSound);

			if (view.Sounds.BackgroundSound != null)
				Play(view.Sounds.BackgroundSound);

			if (view.Sounds.OwnerSoundMode == OwnerSoundMode.Normal)
				return;

			var owner = view.Owner;
			while (owner != null)
			{
				if (owner.Sounds.BackgroundSound != null)
					switch (view.Sounds.OwnerSoundMode)
					{
						case OwnerSoundMode.Fade: Fade(owner.Sounds.BackgroundSound); break;
						case OwnerSoundMode.Mute: Mute(owner.Sounds.BackgroundSound); break;
					}
				owner = owner.Owner;
			}
		}

		private void Mute(AudioClip backgroundSound)
		{
			if (viewSounds.TryGetValue(backgroundSound, out var info))
			{
				info.MuteViewsCounter++;
				StartVolumeChange(info, 0, false);
			}
		}

		private void StopMute(AudioClip backgroundSound)
		{
			if (viewSounds.TryGetValue(backgroundSound, out var info))
			{
				info.MuteViewsCounter--;
				if (info.MuteViewsCounter <= 0)
					StartVolumeChange(info, info.InitVolume, false);
			}
		}

		private void Fade(AudioClip backgroundSound)
		{
			if (viewSounds.TryGetValue(backgroundSound, out var info))
			{
				info.FadeViewsCounter++;
				StartVolumeChange(info, info.InitVolume * FadeVolume, false);
			}
		}

		private void StopFade(AudioClip backgroundSound)
		{
			if (viewSounds.TryGetValue(backgroundSound, out var info))
			{
				info.FadeViewsCounter--;
				if (info.FadeViewsCounter <= 0)
					StartVolumeChange(info, info.InitVolume, false);
			}
		}

		// TODO: Rework and optimize
		private void Play(AudioClip backgroundSound)
		{
			if (viewSounds.TryGetValue(backgroundSound, out var info))
			{
				info.ViewsCounter++;
				return;
			}

			var audio = GameObject.Instantiate(backgroundSoundPrefab);
			audio.transform.SetParent(UIManager.Transform);
			audio.loop = true;
			audio.clip = backgroundSound;
			audio.time = UnityEngine.Random.Range(0, audio.clip.length / 2);
			audio.Play();

			info = new ViewSoundInfo { AudioSource = audio, InitVolume = audio.volume };
			info.ViewsCounter++;
			viewSounds[backgroundSound] = info;

			audio.volume = 0;
			StartVolumeChange(info, info.InitVolume, false);
		}

		private void StopPlay(AudioClip backgroundSound)
		{
			if (viewSounds.TryGetValue(backgroundSound, out var info))
			{
				info.ViewsCounter--;
				if (info.ViewsCounter <= 0)
				{
					StartVolumeChange(info, 0, true);
				}
			}
		}

		private void StartVolumeChange(ViewSoundInfo info, float targetVolume, bool destroyAtEnd)
		{
			if (info.Coroutine != null)
				owner.StopCoroutine(info.Coroutine);

			info.Coroutine = owner.StartCoroutine(VolumeChangeCoroutine(info, targetVolume, destroyAtEnd));
		}

		private IEnumerator VolumeChangeCoroutine(ViewSoundInfo info, float targetVolume, bool destroyAtEnd)
		{
			if (info.Coroutine != null)
				owner.StopCoroutine(info.Coroutine);

			while (info.AudioSource && info.AudioSource.volume != targetVolume)
			{
				info.AudioSource.volume = Mathf.MoveTowards(info.AudioSource.volume, targetVolume, Time.unscaledDeltaTime * VolumeChangeSpeed);
				yield return null;
			}
			if (destroyAtEnd)
			{
				if (info.AudioSource)
				{
					if (viewSounds.TryGetValue(info.AudioSource.clip, out var otherInfo))
					{
						if (info == otherInfo)
							viewSounds.Remove(info.AudioSource.clip);
					}
					info.AudioSource.Stop();
					GameObject.Destroy(info.AudioSource.gameObject);
				}
			}
		}
	}
}