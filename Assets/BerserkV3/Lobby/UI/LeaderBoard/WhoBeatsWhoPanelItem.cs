using RR.UI.FrameSystem;
using Berserk.Shared.Data.Lobby;

namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public partial class WhoBeatsWhoPanelItem : BaseView
	{
		public void SetScrollData(object obj)
		{
			if (obj is not WhoBeatsWhoModel data) return;

			WinnerPlayerNicknamePlayerText.text = data.Winner;
			WinnerPlayerDeltaScoreText.text = data.WinnerScore.ToString();
			LoserPlayerNicknamePlayerText.text = data.Loser;
			LoserPlayerDeltaScoreText.text = data.LoserScore.ToString();
		}
	}
}