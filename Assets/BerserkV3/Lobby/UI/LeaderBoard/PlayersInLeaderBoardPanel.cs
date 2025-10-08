using UnityEngine;
using UnityEngine.UI;
using RR.UI.FrameSystem;
using System.Collections.Generic;
using Berserk.Shared.Data.Lobby;
using UI;

namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public partial class PlayersInLeaderBoardPanel : BaseView
	{
		[SerializeField] private LoopVerticalScrollRect ScrollRect;
		private LoopScrollRefresher refresher;
		private List<PublicLeaderBoardScoreByLeagueModel> currentPlayers = new();
		
		public void SetData(List<PublicLeaderBoardScoreByLeagueModel> players)
		{
			currentPlayers = players ?? new List<PublicLeaderBoardScoreByLeagueModel>();

			ScrollRect.Init(currentPlayers.ToArray(), ScrollRect);

			ScrollRect.TotalCount = currentPlayers.Count;
			ScrollRect.RefillCells();
		}
	}
}
