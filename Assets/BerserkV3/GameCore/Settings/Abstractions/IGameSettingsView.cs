using System;

namespace BerserkV3.GameCore.Settings
{
	public interface IGameSettingsView
	{
		event Action OnQuit;
		event Action OnSurrender;
		event Action OnReport;
		void ActiveSurrender(bool value);
		void ActiveReporting(bool value);
		void ActiveBattle(bool value);
		void Show();
		void Close();
	}
}