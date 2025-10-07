using UnityEngine;
using TMPro;
using Berserk.Shared.Data.Lobby;

namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public class LeaderBoardPanelItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text PlaceText;
		[SerializeField] private TMP_Text UserNameText;
		[SerializeField] private TMP_Text EloText;
		[SerializeField] private TMP_Text WinsText;
		[SerializeField] private TMP_Text GamesText;
		[SerializeField] private TMP_Text AverageTimeText;

		public void SetScrollData(object obj)
		{
			if (obj is not PublicLeaderBoardScoreByLeagueModel data) return;

			PlaceText.text = "-";
			UserNameText.text = data.UserName;
			EloText.text = data.ELO.ToString();
			WinsText.text = data.Wins.ToString();
			GamesText.text = data.Games.ToString();
			AverageTimeText.text = FormatTime((float)data.AverageSessionLengthSec);
		}
		
		private static string FormatTime(float seconds)
		{
			var mins = Mathf.FloorToInt(seconds / 60);
			var secs = Mathf.FloorToInt(seconds % 60);
			return $"{mins:D2}:{secs:D2}";
		}
	}
}
