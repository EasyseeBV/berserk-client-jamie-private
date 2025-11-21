using UnityEngine;
using UnityEngine.UI;
using RR.UI.FrameSystem;
using System.Collections.Generic;
using Berserk.Shared.Data.Lobby;

namespace BerserkV3.Lobby.UI.LeaderBoard
{
	public partial class WhoBeatsWhoLeaderBoardPanel : BaseView
	{
		[SerializeField] private LoopVerticalScrollRect ScrollRect;
		private List<WhoBeatsWhoModel> currentPairs = new();

		public void SetData(List<WhoBeatsWhoModel> pairs)
		{
			currentPairs = pairs ?? new List<WhoBeatsWhoModel>();

			ScrollRect.Init(currentPairs.ToArray(), ScrollRect);
			ScrollRect.TotalCount = currentPairs.Count;
			ScrollRect.RefillCells();
		}
	}
}