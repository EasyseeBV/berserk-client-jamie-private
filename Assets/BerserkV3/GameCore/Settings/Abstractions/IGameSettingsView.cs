using System;

namespace BerserkV3.GameCore.Settings
{
	public interface IGameSettingsView
	{
		event Action OnQuit;
		event Action OnSurrender;
		event Action OnReport;
		event Action<float> OnSoundVolumeChanged;
		event Action<float> OnMusicVolumeChanged;
		
		void SetSoundVolume(float value01);
		void SetMusicVolume(float value01);
		void ActiveSurrender(bool value);
		void ActiveReporting(bool value);
		void ActiveBattle(bool value);
		void Show();
		void Close();
	}
}