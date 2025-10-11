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

		protected override void OnAwake()
		{
			BackButton.Subscribe(Close);
			
			GlobalButton.Subscribe(() => LoadWhoBeatsWhoAsync().Forget());
			LocalButton.Subscribe(() => LoadWhoBeatsWhoByPlayerAsync().Forget());
		}
		

		public void InitAndShow()
		{
			Init();
			Show();
		}
		/*private void OnEnable()
		{
			LoadLeaderBoardAsync().Forget();
		}*/

		private void Init()
		{
			LoadLeaderBoardAsync().Forget();
			LoadWhoBeatsWhoAsync().Forget();
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
		
		private async UniTask LoadWhoBeatsWhoAsync()
		{
			var apiData = await LeaderBoardApplicationAdapter.Application.GetWhoBeatsWho();

			var pairs = apiData
				.Take(25)
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
		
		private async UniTask LoadWhoBeatsWhoByPlayerAsync()
		{
			var username = User.UserName;
			if (string.IsNullOrEmpty(username))
				return;

			var apiData = await LeaderBoardApplicationAdapter.Application.GetWhoBeatsWhoByPlayer(username);

			var pairs = apiData
				.Take(25)
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

