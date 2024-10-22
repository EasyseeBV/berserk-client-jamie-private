using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Berserk.Shared.Data.UserInventory;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Decks;
using BerserkV3.Lobby.Home.Args;
using BerserkV3.Lobby.MatchMaking.AutoMatching;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.Home.CommonWidgets;
using BerserkV3.Lobby.UI.Home.GameModes;
using BerserkV3.Lobby.Vulcanite.Abstractions;
using BerserkV3.Startup.Authorization.Inventory.Models;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Lobby.Home.States.GameModes
{
	public class GameModeLeaguesState : State
	{
		private readonly IInventoryApplication userInventory;
		private readonly IUIService uiService;
		private readonly IGameDatabase gameDatabase;
		private readonly IDeckApplication deckApplication;
		private readonly IAutoMatchApplication autoMatchApplication;
		private readonly IVulcaniteApplication vulcaniteApplication;
		
		private List<LeagueModel> leagues = new();
		private int? selectedDeckIndex;
		private int? selectedLeagueIndex;

		public GameModeLeaguesState(
			IUIService uiService,
			IState parentState,
			IGameDatabase gameDatabase,
			IDeckApplication deckApplication,
			IAutoMatchApplication autoMatchApplication, 
			IInventoryApplication userInventory, 
			IVulcaniteApplication vulcaniteApplication) 
			: base(parentState)
		{
			this.uiService = uiService;
			this.gameDatabase = gameDatabase;
			this.deckApplication = deckApplication;
			this.autoMatchApplication = autoMatchApplication;
			this.userInventory = userInventory;
			this.vulcaniteApplication = vulcaniteApplication;
		}

		public override void OnEnter(params object[] args)
		{
			autoMatchApplication.OnStateChanged += RefreshPage;
			uiService.Begin<GameModeLeagueWindow>()
				.WithInitAsync(InitWindowAsync)
				.Show();
			
			return;
			async Task InitWindowAsync(GameModeLeagueWindow window)
			{
				window.LeaguesSwitcher.Init();
				window.DecksSwitcher.Init();
				
				await FetchLeaguesAsync().AddLoadingTask();
				await UniTask.WhenAll(
					FillDecksAsync(window, CancellationToken.None),
					FillLeaguesAsync(window, CancellationToken.None))  // TODO Token
					.AddLoadingTask();

				window.SetPlayAction(OnMatchStartRequested);
				UpdateTooltip(window);
				RefreshPage();

				// Play Again button - redirection from game
				var playAgainArgs = (LeagueAutoMatchRedirectArg) args.FirstOrDefault(x => x is LeagueAutoMatchRedirectArg);
				var playAgain = playAgainArgs != null;
				var memLastDeckIndex = selectedDeckIndex;
				selectedLeagueIndex = playAgain
					? leagues.FindIndex(x => x.Id == playAgainArgs.LeagueId) 
					: selectedLeagueIndex;
				
				if (!selectedLeagueIndex.HasValue)
				{
					selectedDeckIndex = null;
					return;
				}
				
				OnLeagueSelected(selectedLeagueIndex.Value);
				window.LeaguesSwitcher.SwitchWithoutNotify(selectedLeagueIndex.Value);
				
				selectedDeckIndex = playAgain
					? deckApplication.All.ToList().FindIndex(x => x.Id == playAgainArgs.DeckId)
					: memLastDeckIndex;
				
				if (!selectedDeckIndex.HasValue)
					return;
				
				OnDeckSelected(selectedDeckIndex.Value);
				window.DecksSwitcher.SwitchWithoutNotify(selectedDeckIndex.Value);
				
				if (playAgain)
					OnMatchStartRequested();
			}
		}

		private void OnMatchStartRequested()
		{
			if (autoMatchApplication.State.IsAutoMatchJoined)
			{
				autoMatchApplication.LeaveAutoMatch();
				UpdatePlayButton(uiService.Get<GameModeLeagueWindow>(), true);
				return;
			}
			
			autoMatchApplication.JoinAutoMatchAsync(new AutoMatchJoinModel
			{
				DeckId = deckApplication.All.ElementAt(selectedDeckIndex!.Value).Id,
				LeagueId = leagues.ElementAt(selectedLeagueIndex!.Value).Id,
				MatchMode = MatchMode.Ranked
			}).Forget();
			
			UpdatePlayButton(uiService.Get<GameModeLeagueWindow>(), true);
		}

		private async UniTask FetchLeaguesAsync()
		{
			var leagueResponce = await LobbyAPI.GetAvailableLeagues();
			if (!leagueResponce)
			{
				// TODO handle failed fetch
				return;
			}
			
			leagues = leagueResponce.Data;
		}

		private async UniTask FillLeaguesAsync(GameModeLeagueWindow window, CancellationToken token)
		{
			var leagueSwitcher = window.LeaguesSwitcher;
			leagueSwitcher.OnToggleSelected -= OnLeagueSelected;
			leagueSwitcher.OnToggleSelected += OnLeagueSelected;
			leagueSwitcher.AddOrRefresh(leagues.Select(x => x.LeagueName).ToList(), -1);

			await UniTask.WhenAll(leagueSwitcher.Toggels
				.Where(x => x.gameObject.activeSelf)
				.Select(toggle => toggle.GetComponentInChildren<LeagueWidget>(true))
				.Select((widget, index) =>
				{
					var leagueModel = leagues.ElementAt(index);
					return widget.InitAsync(leagueModel.GetArtURL(), token);
				}));
			
			leagueSwitcher.DisableAll();
		}

		private async UniTask FillDecksAsync(GameModeLeagueWindow window, CancellationToken token)
		{
			var deckSwitcher = window.DecksSwitcher;
			deckSwitcher.OnToggleSelected -= OnDeckSelected;
			deckSwitcher.OnToggleSelected += OnDeckSelected;
			await UniTask.WhenAll(deckSwitcher.Toggels
				.Select(toggle => (toggle, widget:toggle.GetComponentInChildren<CompactDeckWidget>(true)))
				.Select((tuple, index) =>
				{
					var deckData = deckApplication.All.ElementAtOrDefault(index);
					var isValidDeck = IsValidDeckCurrentLeague(deckData);
					var isDeckExist = deckData != null;
					
					deckSwitcher.AddOrRefresh(tuple.toggle, deckData?.Name, index);
					tuple.widget.SetActive(isDeckExist);
					tuple.toggle.interactable = isValidDeck;
					
					if (!isDeckExist || tuple.widget.Initialized)
						return UniTask.CompletedTask;
					
					var ownedVulcanite = vulcaniteApplication.Get(deckData!.OwnedVulcaniteId);
					var heroData = gameDatabase.GetHero(ownedVulcanite?.VulcaniteId);
					return tuple.widget.InitAsync(heroData?.ArtUrl, $"{heroData?.Quadrant}_Flag", token);
				}));
			
			deckSwitcher.DisableAll();
		}

		private void LockSelectors(GameModeLeagueWindow window, bool isLocked)
		{
			window.DecksSwitcher.SetInteractable(!isLocked);
			window.LeaguesSwitcher.SetInteractable(!isLocked);
		}

		private void OnDeckSelected(int deckIndex)
		{
			selectedDeckIndex = deckIndex;
			RefreshPage();
		}

		private void OnLeagueSelected(int leagueIndex)
		{
			var window = uiService.Get<GameModeLeagueWindow>();
			selectedLeagueIndex = leagueIndex;
			selectedDeckIndex = null;
			UpdateTooltip(window);
			FillDecksAsync(window, CancellationToken.None).Forget();
			RefreshPage();
		}

		private void UpdateTooltip(GameModeLeagueWindow window)
		{
			var leagueModel = leagues.ElementAtOrDefault(selectedLeagueIndex ?? -1);
			window.SetTooltipHeaderText(leagueModel?.LeagueName ?? "");
			window.SetTooltipText(leagueModel?.LeagueDescription ?? "Select a league and deck to play."); // todo localize
		}

		private bool IsValidDeckCurrentLeague(OwnedDeck ownedDeck)
		{
			if (!deckApplication.IsValid(ownedDeck) || !selectedLeagueIndex.HasValue || selectedLeagueIndex < 0)
				return false;

			var selectedLeague = leagues.ElementAtOrDefault(selectedLeagueIndex.Value);
			var isDeckValid = deckApplication.IsValidForLeague(selectedLeague, ownedDeck);
			return isDeckValid;
		}

		private void UpdatePlayButton(GameModeLeagueWindow window, bool waitStateRefresh = false)
		{
			var inSearch = autoMatchApplication.State.IsAutoMatchJoined;
			var buttonText = inSearch ? "Cancel" : "Play"; // TODO localize
			window.SetPlayButtonText(buttonText);
			
			if (inSearch)
			{
				window.SetPlayInteractable(!waitStateRefresh);
				return;
			}
			
			window.SetPlayInteractable(selectedDeckIndex.HasValue && selectedLeagueIndex.HasValue && !waitStateRefresh);
		}

		private void RefreshPage()
		{
			var window = uiService.Get<GameModeLeagueWindow>();
			UpdatePlayButton(window);
			LockSelectors(window, autoMatchApplication.State.IsAutoMatchJoined || autoMatchApplication.State.IsMatchFound);
		}

		public override void OnExit()
		{
			autoMatchApplication.OnStateChanged -= RefreshPage;
			uiService.Begin<GameModeLeagueWindow>()
				.WithInit(CallBack)
				.Hide();
			
			base.OnExit();
			return;
			void CallBack(GameModeLeagueWindow window)
			{
				foreach (var toggle in window.DecksSwitcher.Toggels)
					toggle.GetComponentInChildren<CompactDeckWidget>(true).Clear();
				
				foreach (var toggle in window.LeaguesSwitcher.Toggels)
					toggle.GetComponentInChildren<LeagueWidget>(true).Clear();
				
				window.DecksSwitcher.Release();
				window.LeaguesSwitcher.Release();
			}
		}
	}
}