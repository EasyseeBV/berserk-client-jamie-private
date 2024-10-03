using System;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using Events;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;

namespace UI
{
	[Obsolete ("Need to rework")]
	public partial class SettingsView : BaseView
	{
		protected override void Start()
		{
			base.Start();
			MusicSlider.onValueChanged.AddListener(OnMusicVolumeUpdate);
			AudioSlider.onValueChanged.AddListener(OnAudioVolumeUpdate);
			CloseBtn.Subscribe(Close);
			RestartTutorBtn.Subscribe(RestartTutorial);
			RestartTutorBtn.Subscribe(Close);
			SceneServiceAdapter.Service.OnSceneLoaded += OnSceneLoaded;
			OnSceneLoaded(SceneServiceAdapter.Service.Current);
		}

		private void OnSceneLoaded(Scene scene)
		{
			var inGame = scene == Scene.Game;
			SetActive(RestartTutorBtn, !inGame);
		}

		private async void RestartTutorial()
		{
			await TutorialAdapter.Application.RestartAsync().AddLoadingTask();
			await TutorialAdapter.Application.InvokeAsync(TutorialTrigger.StartSession.ToString()).AddLoadingTask();
		}

		protected override void OnShown()
		{
			AudioSlider.value = DataBus.AppData.Value.AudioVolume;
			MusicSlider.value = DataBus.AppData.Value.MusicVolume;
		}

		private void OnMusicVolumeUpdate(float volume)
		{
			DataBus.AppData.Value.MusicVolume = volume;
			DataBus.AppData.Repeat();
		}

		private void OnAudioVolumeUpdate(float volume)
		{
			DataBus.AppData.Value.AudioVolume = volume;
			DataBus.AppData.Repeat();
		}

		private void OnDestroy()
		{
			SceneServiceAdapter.Service.OnSceneLoaded -= OnSceneLoaded;
			MusicSlider.onValueChanged.RemoveAllListeners();
			AudioSlider.onValueChanged.RemoveAllListeners();
			CloseBtn.onClick.RemoveAllListeners();
			RestartTutorBtn.onClick.RemoveAllListeners();
			RestartTutorBtn.onClick.RemoveAllListeners();
		}
	}
}