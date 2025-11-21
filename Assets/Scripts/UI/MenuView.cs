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
			LavaShopBtn.Subscribe(() => LavaShopView.Instance.InitAndShowAsync().Forget());
			LeaderBoardBtn.onClick.AddListener(()=> LeaderBoardLeagueView.Instance.InitAndShow());
			LavaShopBtn.ShowAtStart = true;
			LavaShopBtn.SetInteractable(false); // turn off lava shop (todo: remove when new shop will be developed)

			SettingsButton.onClick.AddListener(() => SettingsView.Instance.Show());
			LobbyBus.CurrentOnlineCount.Subscribe(this, UpdateOnlineView);
			UpdateOnlineView(LobbyBus.CurrentOnlineCount.Value);
			
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
			LavaShopBtn.gameObject.SetActive(true);
		}

		private void UpdateOnlineView(int count)
		{
			OnlineText.SetText($"Online: {count}");
		}

		private void OnDestroy()
		{
			disposables.ForEach(x=> x.Dispose());
			disposables.Clear();
		}
	}
}
