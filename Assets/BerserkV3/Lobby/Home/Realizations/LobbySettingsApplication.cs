using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.UI;
using RR.UIService;

namespace BerserkV3.Lobby.Home
{
	public class LobbySettingsApplication : ILobbySettingsApplication
	{
		private readonly IUIService uiService;
		private readonly IAudioApplication audioApplication;
		private readonly IBerserkTutorialApplication tutorialApplication;

		public LobbySettingsApplication(
			IUIService uiService,
			IAudioApplication audioApplication,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.uiService = uiService;
			this.audioApplication = audioApplication;
			this.tutorialApplication = tutorialApplication;
		}

		public void Init()
		{
			uiService.Begin<LobbySettingsButtonWindow>()
				.WithInit(window => window.SetClickAction(OpenSettings))
				.Show();
		}

		private void OpenSettings()
		{
			uiService.Begin<LobbySettingsWindow>()
				.WithInit(InitWindow)
				.Show();

			return;

			void InitWindow(LobbySettingsWindow window)
			{
				var soundInit = audioApplication.SoundVolume;
				var musicInit = audioApplication.MusicVolume;

				window.SetSound(soundInit, audioApplication.SetSoundVolume);
				window.SetMusic(musicInit, audioApplication.SetMusicVolume);

				window.SetCancelAction(ResetValues);
				window.SetCloseAction(ResetValues);

				window.SetApplyAction(CloseSettings);
				window.SetCloseAction(CloseSettings);
				window.SetTutorialAction(ReplayTutorialAsync);

				return;

				void ResetValues()
				{
					audioApplication.SetSoundVolume(soundInit);
					audioApplication.SetMusicVolume(musicInit);
					window.SetSound(soundInit);
					window.SetMusic(musicInit);
				}
			}
		}

		private async void ReplayTutorialAsync()
		{
			await tutorialApplication.InitAsync().AddLoadingTask();
			await tutorialApplication.ResetAsync().AddLoadingTask();
			await tutorialApplication.InvokeAsync(TutorialTrigger.StartSession).AddLoadingTask();
		}

		private void CloseSettings()
		{
			uiService.Begin<LobbySettingsWindow>().Hide();
		}
	}
}