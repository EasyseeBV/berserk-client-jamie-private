using System;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.GameCore.Settings;
using BerserkV3.Startup.UI;
using Events;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.UI
{
	public partial class GameSettingsView : SafeView, IGameSettingsView
	{
		public event Action OnQuit;
		public event Action OnSurrender;
		public event Action OnReport;

		protected override void OnAwake()
		{
			base.OnAwake();
			MusicSlider.onValueChanged.AddListener(OnMusicVolumeUpdate);
			AudioSlider.onValueChanged.AddListener(OnAudioVolumeUpdate);
			
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
		}
	}
}