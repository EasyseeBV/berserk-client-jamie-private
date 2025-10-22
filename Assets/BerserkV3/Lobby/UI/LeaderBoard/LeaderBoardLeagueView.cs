using RR.UI.FrameSystem;
using Cysharp.Threading.Tasks;
using BerserkV3.Lobby.LeaderBoard;
using BerserkV3.Startup.Authorization;
using Berserk.Shared.Data.Lobby;
using System.Linq;


namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public partial class LeaderBoardLeagueView : BaseView
	{
		private const string HARDCODE_LEAGUE_ID = "65da744a-852a-4d9b-879d-7ef37df7e558";
		private const string HARDCODE_LEAGUE_NAME = "RANKED";
		private const string HARDCODE_SEASON_ID = "spring-2023";
		
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

		private void Init()
		{
			SetActiveButton(true);
			LoadLeaderBoardAsync().Forget();
			LoadWhoBeatsWhoAsync().Forget();
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
				LeagueName.text = HARDCODE_LEAGUE_NAME;
				LastSeasonButtonText.text = "Last Season";
				LoadLeaderBoardAsync().Forget();
			}
			else
			{
				isLastSeason = true;
				LeagueName.text = HARDCODE_SEASON_ID;
				LastSeasonButtonText.text = HARDCODE_LEAGUE_NAME;
				LoadLeaderBoardBySeasonAsync().Forget();
			}
		}
		
		private async UniTask LoadLeaderBoardAsync()
		{
			var apiData = await LeaderBoardApplicationAdapter.Application.GetLeagueLeaderBoard(HARDCODE_LEAGUE_ID);

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
			var apiData = await LeaderBoardApplicationAdapter.Application.GetLeagueLeaderBoardBySeason(HARDCODE_LEAGUE_ID, HARDCODE_SEASON_ID);

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

