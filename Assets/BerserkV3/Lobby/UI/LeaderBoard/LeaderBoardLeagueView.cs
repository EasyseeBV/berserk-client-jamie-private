using RR.UI.FrameSystem;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BerserkV3.Lobby.LeaderBoard;
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
		}
		

		private async UniTask LoadLeaderBoardAsync()
		{
			var apiData = await LeaderBoardApplicationAdapter.Application.GetLeagueLeaderBoard(HARDCODE_LEAGUE_ID);

			var players = apiData
				.Take(25)
				.Select(p => new PublicLeaderBoardScoreByLeagueModel
				{
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
	}
}

