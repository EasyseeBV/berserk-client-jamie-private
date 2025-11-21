using RR.UI.FrameSystem;
using Cysharp.Threading.Tasks;
using BerserkV3.Lobby.LeaderBoard;
using BerserkV3.Startup.Authorization;
using Berserk.Shared.Data.Lobby;
using System.Linq;
using System;


namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public partial class LeaderBoardLeagueView : BaseView
	{
		private string currentLeagueId;
		private string currentLeagueName;
		private string currentSeasonId;
		
		private bool isLastSeason = false;
		private bool isGlobalActive = true;

		protected override void OnAwake()
		{
			BackButton.Subscribe(Close);
			
			GlobalButton.Subscribe(() => {
				SetActiveButton(true);
				LoadWhoBeatsWhoAsync().Forget();
			});
			
			LocalButton.Subscribe(() => {
				SetActiveButton(false);
				LoadWhoBeatsWhoByPlayerAsync().Forget();
			});
			
			LastSeasonButton.Subscribe(OnLastSeasonClicked);
		}
		
		public void InitAndShow()
		{
			Init();
			Show();
		}

		private async UniTask Init()
		{
			SetActiveButton(true);
				
			var meta = await LeaderBoardApplicationAdapter.Application.GetLeaderBoardMeta();
			if (meta != null && meta.Leagues != null && meta.Leagues.Count > 0)
			{
				var ranked = meta.Leagues.FirstOrDefault(l =>
					             l.LeagueName.Equals("Ranked", StringComparison.OrdinalIgnoreCase))
				             ?? meta.Leagues[0];

				currentLeagueId = ranked.LeagueId;
				currentLeagueName = ranked.LeagueName;
				currentSeasonId = meta.SeasonId;

				LeagueName.text = currentLeagueName;
			}
			
			await LoadLeaderBoardAsync();
			await LoadWhoBeatsWhoAsync();
		}
		
		private void SetActiveButton(bool globalActive)
		{
			isGlobalActive = globalActive;
			
			GlobalButton.SetInteractable(!globalActive);
			LocalButton.SetInteractable(globalActive);
			
			GlobalOutlineImg.gameObject.SetActive(globalActive);
			LocalOutlineImg.gameObject.SetActive(!globalActive);
			
		}
		
		private void OnLastSeasonClicked()
		{
			if (isLastSeason)
			{
				isLastSeason = false;
				LeagueName.text = currentLeagueName;
				LastSeasonButtonText.text = "Last Season";
				LoadLeaderBoardAsync().Forget();
			}
			else
			{
				isLastSeason = true;
				LeagueName.text = currentSeasonId;
				LastSeasonButtonText.text = currentSeasonId;
				LoadLeaderBoardBySeasonAsync().Forget();
			}
		}
		
		private async UniTask LoadLeaderBoardAsync()
		{
			var apiData = await LeaderBoardApplicationAdapter.Application.GetLeagueLeaderBoard(currentLeagueId);

			var players = apiData
				.Take(25)
				.Select((p, index) => new PublicLeaderBoardScoreByLeagueModel
				{
					Rank = index + 1,
					UserName = p.UserName,
					ELO = p.ELO,
					LeagueName = p.LeagueName,
					Wins = p.Wins,
					Loses = p.Loses,
					Games = p.Games,
					AverageSessionLengthSec = (float)p.AverageSessionLengthSec
				})
				.ToList();

			PlayersInLeaderBoardPanel.SetData(players);
		}
		
		private async UniTask LoadLeaderBoardBySeasonAsync()
		{
			var apiData = await LeaderBoardApplicationAdapter.Application.GetLeagueLeaderBoardBySeason(currentLeagueId, currentSeasonId);

			var players = apiData
				.Take(25)
				.Select((p, index) => new PublicLeaderBoardScoreByLeagueModel
				{
					Rank = index + 1,
					UserName = p.UserName,
					ELO = p.ELO,
					LeagueName = p.LeagueName,
					Wins = p.Wins,
					Loses = p.Loses,
					Games = p.Games,
					AverageSessionLengthSec = (float)p.AverageSessionLengthSec
				})
				.ToList();

			PlayersInLeaderBoardPanel.SetData(players);
		}
		
		private async UniTask LoadWhoBeatsWhoAsync(int limit = 25)
		{
			var apiData = await LeaderBoardApplicationAdapter.Application.GetWhoBeatsWho(limit);

			var pairs = apiData
				.Take(limit)
				.Select(p => new WhoBeatsWhoModel
				{
					Winner = p.Winner,
					WinnerScore = p.WinnerScore,
					Loser = p.Loser,
					LoserScore = p.LoserScore
				})
				.ToList();

			WhoBeatsWhoLeaderBoardPanel.SetData(pairs);
		}
		
		private async UniTask LoadWhoBeatsWhoByPlayerAsync(int limit = 25)
		{
			var username = User.UserName;
			if (string.IsNullOrEmpty(username))
				return;

			var apiData = await LeaderBoardApplicationAdapter.Application.GetWhoBeatsWhoByPlayer(username, limit);

			var pairs = apiData
				.Take(limit)
				.Select(p => new WhoBeatsWhoModel
				{
					Winner = p.Winner,
					WinnerScore = p.WinnerScore,
					Loser = p.Loser,
					LoserScore = p.LoserScore
				})
				.ToList();

			WhoBeatsWhoLeaderBoardPanel.SetData(pairs);
		}
	}
}

