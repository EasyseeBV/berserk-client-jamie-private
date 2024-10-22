using System;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.GameCore.Settings;
using BerserkV3.Startup.UI;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.UI
{
	public partial class GameSettingsView : SafeView, IGameSettingsView
	{
		public event Action OnQuit;
		public event Action OnSurrender;
		public event Action OnReport;

		public event Action<float> OnSoundVolumeChanged;
		public event Action<float> OnMusicVolumeChanged;

		protected override void OnAwake()
		{
			base.OnAwake();
			MusicSlider.onValueChanged.AddListener(value => OnMusicVolumeChanged?.Invoke(value));
			AudioSlider.onValueChanged.AddListener(value => OnSoundVolumeChanged?.Invoke(value));
			
			CloseBtn.onClick.AddListener(Close);
			SurrenderBtn.onClick.AddListener(() => OnSurrender?.Invoke());
			QuitBtn.onClick.AddListener(() => OnQuit?.Invoke());
			ReportBtn.onClick.AddListener(() => OnReport?.Invoke());
			DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(OnAwake)} method, {nameof(SurrenderBtn)} subscribed {nameof(OnSurrender)} events!");
		}

		public void ActiveSurrender(bool value)
		{
			SetActive(SurrenderBtn, value);
			SetActive(QuitBtn, !value);
		}

		public void ActiveReporting(bool value)
		{ 
			SetActive(ReportBtn, value);
		}
		
		public void ActiveBattle(bool value)
		{
			SetActive(BattleButtonsPanel, value);
		}

		public void SetSoundVolume(float value01)
		{
			AudioSlider.value = value01;
		}

		public void SetMusicVolume(float value01)
		{
			MusicSlider.value = value01;
		}

		private void OnDestroy()
		{
			DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(OnDestroy)} method, {nameof(SurrenderBtn)} Unsubscribed and clean {nameof(OnSurrender)} event!");
			MusicSlider.onValueChanged.RemoveAllListeners();
			AudioSlider.onValueChanged.RemoveAllListeners();
			CloseBtn.onClick.RemoveAllListeners();
			SurrenderBtn.onClick.RemoveAllListeners();
			QuitBtn.onClick.RemoveAllListeners();
			ReportBtn.onClick.RemoveAllListeners();
			OnQuit = null;
			OnSurrender = null;
			OnReport = null;
			OnSoundVolumeChanged = null;
			OnMusicVolumeChanged = null;
		}
	}
}