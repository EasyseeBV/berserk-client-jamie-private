using System;
using System.Collections.Generic;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Lobby.UI.LeaderBoard;
using BerserkV3.Lobby.UI.Leagues;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;

namespace UI
{
	public partial class MenuView : BaseView
	{
		private readonly List<IDisposable> disposables = new();
		protected override void OnAwake()
		{
			LogOutBtn.Subscribe(Logout);
			DeckBtn.Subscribe(() => PlayerDecksView.Instance.InitAndShow());
			PlayBtn.Subscribe(() => LobbyLeaguesView.Instance.InitAndShowAsync().Forget());
			DuelsBtn.Subscribe(() => DuelsApplicationAdapter.Application.OpenAsync().Forget());
			ProfileBtn.Subscribe(() => ProfileView.Instance.InitAndShowAsync().Forget());
			LeaderBoardBtn.onClick.AddListener(()=> LeaderBoardLeagueView.Instance.InitAndShow());
			LavaShopBtn.ShowAtStart = false;
			LavaShopBtn.gameObject.SetActive(false);

			SettingsButton.onClick.AddListener(ShowSettings);
			LobbyBus.CurrentOnlineCount.Subscribe(this, UpdateOnlineView);
			UpdateOnlineView(LobbyBus.CurrentOnlineCount.Value);
			InitRedesignButtons();
			InitSoloMenuShell();
			
			disposables.Add(DeckBtn.SetHintTarget(TutorialTrigger.DecksBtn.ToString()).SetTransitionFactorSize().Init().Subscribe(DeckBtn.Get()));
			disposables.Add(PlayBtn.SetHintTarget(TutorialTrigger.LeaguesBtn.ToString()).SetTransitionFactorSize().Init().Subscribe(PlayBtn.Get()));
			disposables.Add(DuelsBtn.SetHintTarget(TutorialTrigger.DuelsBtn.ToString()).SetTransitionFactorSize().Init().Subscribe(DuelsBtn.Get()));
		}

		private void Logout()
		{
			ConfirmationDialog.Instance.Init()
				.SetMessage(GameDataBaseAdapter.Instance.GetLocalization("AccountLogout"))
				.SetClose("Close")
				.SetOk("Exit")
				.SetOkColor(Color.red)
				.SetCancelColor(Color.red)
				.SetCloseColor(Color.white)
				.SetCancel("Logout")
				.SetResponseCancel(() => LobbyBus.LogOut += true)
				.SetResponseOk(() => Application.Quit(0))
				.Apply();
		}

		protected override void OnShown()
		{
			LavaShopBtn.gameObject.SetActive(false);
		}

		private void UpdateOnlineView(int count)
		{
			OnlineText.SetText($"Online: {count}");
		}

		private static void ShowSettings()
		{
			var view = SettingsView.Instance;
			if (view == null && UIManager.StaticViews.TryGetValue(nameof(SettingsView), out var staticView))
				view = staticView as SettingsView;

			if (view == null)
			{
				var views = Resources.FindObjectsOfTypeAll<SettingsView>();
				if (views != null && views.Length > 0)
					view = views[0];
			}

			if (view != null)
				view.Show();
			else
				Debug.LogError("SettingsView could not be resolved from MenuView.ShowSettings()");
		}

		private void OnDestroy()
		{
			disposables.ForEach(x=> x.Dispose());
			disposables.Clear();
		}
	}
}
