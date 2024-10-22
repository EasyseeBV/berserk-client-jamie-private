using System;
using System.Threading;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RR.Core.ResourceManagament;
using RR.UIService.Data;
using UnityEngine;
using Zenject;

namespace BerserkV3.Common.AudioSystem
{
	public class BerserkAudioApplication : IAudioApplication, IInitializable, IDisposable
	{
		private class AudioData
		{
			public float SoundVolume = 0.5f;
			public float MusicVolume = 1f;
		}
		
		private readonly IAudioPlayer soundPlayer;
		private readonly IAudioPlayer musicPlayer;
		private readonly ISceneService sceneService;
		private readonly ISerializeHelper serializeHelper;
		private readonly IResourceService resourceService;
		private readonly ICustomisationItemRepository custRepository;
		
		private AudioData data;
		private CancellationTokenSource musicSource;
		private AudioClip lastLoadedMusic;
		private AudioClip lastLoadedSound;
		private string lastMisicPlayed;
		
		public float SoundVolume => data.SoundVolume;
		public float MusicVolume => data.MusicVolume;

		public BerserkAudioApplication(
			IAudioPlayer soundPlayer, 
			IAudioPlayer musicPlayer,
			ISceneService sceneService,
			ISerializeHelper serializeHelper,
			IResourceService resourceService,
			ICustomisationItemRepository custRepository)
		{
			this.soundPlayer = soundPlayer;
			this.musicPlayer = musicPlayer;
			this.sceneService = sceneService;
			this.serializeHelper = serializeHelper;
			this.resourceService = resourceService;
			this.custRepository = custRepository;
		}


		public void Initialize()
		{
			Restore();
			sceneService.OnSceneLoaded += OnSceneLoaded;
		}

		public void Dispose()
		{
			sceneService.OnSceneLoaded -= OnSceneLoaded;
		}

		public void SetSoundVolume(float value01)
		{
			data.SoundVolume = Mathf.Clamp01(value01);
			soundPlayer.SetVolume(SoundVolume);
			Serialize();
		}

		public void SetMusicVolume(float value01)
		{
			data.MusicVolume = Mathf.Clamp01(value01);
			musicPlayer.SetVolume(MusicVolume);
			Serialize();
		}

		public void PlaySound(object id)
		{
			if (id is UIAudioClipData uiClip)
			{
				soundPlayer.Play(uiClip.clip, false);
				return;
			}
			
			var clipId = GetClipId(id);
		
			if (clipId == null)
				return;
			
			PlayAsync(soundPlayer, clipId, false, false, lastLoadedSound)
				.ContinueWith(clip => lastLoadedSound = clip)
				.Forget();
		}
		
		public void PlayMusic(object id)
		{
			var musicId = GetClipId(id);

			if (string.IsNullOrEmpty(musicId) || lastMisicPlayed == musicId)
				return;

			lastMisicPlayed = musicId;
			musicSource?.Cancel();
			musicSource?.Dispose();
			musicSource = new CancellationTokenSource();
			
			PlayAsync(musicPlayer, musicId, true, true, lastLoadedMusic, musicSource.Token)
				.ContinueWith(clip => lastLoadedMusic = clip)
				.Forget();
		}

		private async UniTask<AudioClip> PlayAsync(
			IAudioPlayer player, 
			string id, 
			bool stopPrevious,
			bool loop,
			AudioClip previous, 
			CancellationToken token = default)
		{
			if (id == Clip.None.ToString())
				return previous;
			
			var clip = await resourceService.GetAsync<AudioClip>(id, token);
			
			if (previous)
				previous.ReleaseResource();
			
			if (stopPrevious)
				player.Stop();
			
			player.Play(clip, loop);
			return clip;
		}

		private string GetClipId(object id)
		{
			return id switch
			{
				CustomisationType type => custRepository.GetFirstEquipped(type)?.PreviewURL,
				Clip or int or string => id.ToString(),
				_ => null
			};
		}
		
		private void Restore()
		{
			data = serializeHelper.HasKey(nameof(BerserkAudioApplication))
				? JsonConvert.DeserializeObject<AudioData>(serializeHelper.Get(nameof(BerserkAudioApplication)))
				: new AudioData();
			SetSoundVolume(data.SoundVolume);
			SetMusicVolume(data.MusicVolume);
			Serialize();
		}

		private void Serialize()
		{
			var json = JsonConvert.SerializeObject(data);
			serializeHelper.Patch(nameof(BerserkAudioApplication), json);
			serializeHelper.Save();
		}
		
		private void OnSceneLoaded(Scene current)
		{
			switch (current)
			{
				case Scene.Lobby:
				{
					PlayMusic(CustomisationType.LobbyMusic);
					break;
				}
				
				case Scene.Game:
				{
					PlayMusic(CustomisationType.BattleMusic);
					break;
				}
				
				case Scene.StartUp:
				{
					PlayMusic(Clip.MainTheme);
					break;
				}

				case Scene.Init:
				case Scene.ShowRoom:
				default: return;
			}
		}
	}
}