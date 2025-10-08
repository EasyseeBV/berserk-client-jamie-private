using UnityEngine;
using TMPro;
using Berserk.Shared.Data.Lobby;
using RR.UI.FrameSystem;

namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public partial class LeaderBoardPanelItem : BaseView
	{
		public void SetScrollData(object obj)
		{
			if (obj is not PublicLeaderBoardScoreByLeagueModel data) return;

			PlacePlayerText.text = "-";
			NicknamePlayerText.text = data.UserName;
			ELOPlayerText.text = data.ELO.ToString();
			WinsPlayerText.text = data.Wins.ToString();
			GamesPlayerText.text = data.Games.ToString();
			AverageSessionLengthPlayerText.text = FormatTime((float)data.AverageSessionLengthSec);
		}
		
		private static string FormatTime(float seconds)
		{
			var mins = Mathf.FloorToInt(seconds / 60);
			var secs = Mathf.FloorToInt(seconds % 60);
			return $"{mins:D2}:{secs:D2}";
		}
	}
}
