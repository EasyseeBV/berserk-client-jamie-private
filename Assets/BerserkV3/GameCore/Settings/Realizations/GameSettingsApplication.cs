using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using RR.Core.Extensions;
using UI;
using Zenject;

namespace BerserkV3.GameCore.Settings
{
	public class GameSettingsApplication : IInitializable, IDisposable
	{
		private readonly IReportService reportService;
		private readonly IAudioApplication audioApplication;
		private readonly IGameHub gameHub;
		private readonly IGameContext gameContext;
		private readonly IGameSettingButtonView settingButtonView;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly ISceneService sceneService;
		private readonly IGameSettingsView settingsView;
		private readonly IReportView reportView;
		private bool initialized;

		public GameSettingsApplication(
			IGameHub gameHub,
			IGameContext gameContext,
			IReportService reportService,
			IAudioApplication audioApplication,
			IGameSettingButtonView settingButtonView,
			IBerserkTutorialApplication tutorialApplication,
			IGameLogicEventsSource gameLogicEventsSource,
			ISceneService sceneService,
			IGameSettingsView settingsView,
			IReportView reportView)
		{
			this.reportService = reportService;
			this.audioApplication = audioApplication;
			this.gameHub = gameHub;
			this.gameContext = gameContext;
			this.settingButtonView = settingButtonView;
			this.tutorialApplication = tutorialApplication;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.sceneService = sceneService;
			this.settingsView = settingsView;
			this.reportView = reportView;
		}

		public void Initialize()
		{
			DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(Initialize)} method, subscribe to {nameof(OnSurrender)}.");
			settingsView.OnSurrender += OnSurrender;
			DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(Initialize)} method, {nameof(OnSurrender)} subscribed!");
			settingsView.OnReport += reportView.Show;
			settingsView.OnQuit += HandleGameLeft;
			settingsView.OnSoundVolumeChanged += audioApplication.SetSoundVolume;
			settingsView.OnMusicVolumeChanged += audioApplication.SetMusicVolume;
			reportView.OnReport += Reporting;
			settingButtonView.OnClick += Show;
			gameLogicEventsSource.Subscribe<InitializeGame>(_ =>
			{
				initialized = !gameContext.RuntimeData.IsEnded;
				settingsView.ActiveSurrender(initialized);
			});
		}

		public void Dispose()
		{
			if (settingsView != null)
			{
				DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(Dispose)} method, {nameof(OnSurrender)} Unsubscribed!");
				settingsView.OnSurrender -= OnSurrender;
				settingsView.OnReport -= reportView.Show;
				settingsView.OnQuit -= HandleGameLeft;
				settingsView.OnSoundVolumeChanged -= audioApplication.SetSoundVolume;
				settingsView.OnMusicVolumeChanged -= audioApplication.SetMusicVolume;
			}

			if (reportView != null)
				reportView.OnReport -= Reporting;
			
			if (settingButtonView != null)
				settingButtonView.OnClick -= Show;
		}

		private void Show()
		{
			settingsView.SetSoundVolume(audioApplication.SoundVolume);
			settingsView.SetMusicVolume(audioApplication.MusicVolume);
			settingsView.ActiveBattle(true);
			settingsView.ActiveReporting(reportService.IsAvailable);
			settingsView.ActiveSurrender(initialized);
			settingsView.Show();
		}

		private async void Reporting(ReportReason reason)
		{
			await reportService.SendReportAsync(reason).AddLoadingTask();
			reportView.Close();
		}

		private void OnSurrender()
		{
			DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(OnSurrender)} method, clicked and ready to open the {nameof(ConfirmationDialog)}.");
			ConfirmationDialog.Instance.Init()
				.SetMessage(gameContext.GameDatabase.GetLocalization("ResignSurrender"))
				.SetResponse(result => DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(OnSurrender)} method, closed the {nameof(ConfirmationDialog)} with result to send surrender: {result}."))
				.SetResponseOk(SendSurrender)
				.Apply();
			
			DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(OnSurrender)} method, the {nameof(ConfirmationDialog)} Opened!");
		}

		private async void SendSurrender()
		{
			DefaultSharedLogger.Log($"[{GetType().Name.Orange().Bold()}] {nameof(SendSurrender)} method, approved and ready to send.");
			settingsView.Close();
			var param = new GameEndParams {Reason = GameEndReason.Surrender};
			var model = new CmdParamsModel(gameContext.Timer.RuntimeData.TimeHash, param);
			await gameHub.PerformCommandAsync<GameEndCmd>(model).AddLoadingTask();
		}

		private async void HandleGameLeft()
		{
			try
			{
				if (gameContext.RuntimeData is {MatchMode: MatchMode.Tutorial})
					await tutorialApplication.ResetAsync().AddLoadingTask();
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
			finally
			{
				await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
			}
		}
	}
}