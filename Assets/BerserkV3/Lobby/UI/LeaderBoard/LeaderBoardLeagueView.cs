using RR.UI.FrameSystem;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BerserkV3.Lobby.LeaderBoard;
using Berserk.Shared.Data.Lobby;
using System.Linq;
using UI;

namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public class LeaderBoardLeagueView : BaseView
	{
		[SerializeField] private ButtonView BackButton;
		[SerializeField] private PlayersInLeaderBoardPanel PlayersPanel;
		private const string HARDCODE_LEAGUE_ID = "65da744a-852a-4d9b-879d-7ef37df7e558";

		private void OnEnable()
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

			PlayersPanel.SetData(players);
		}
	}
}

