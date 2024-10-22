using System.Collections.Generic;
using BerserkV3.Common.UIKit;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.GameModes.Widgets
{
	public class DraftRewardsWidget : UIViewBase
	{
		[SerializeField] private List<DraftRewardItemWidget> rewardWidgets = new();
		[SerializeField] private List<DraftRewardGemWidget> gemWidgets = new();

		public List<DraftRewardItemWidget> RewardWidgets => rewardWidgets;
		public List<DraftRewardGemWidget> GemWidgets => gemWidgets;
		
		public void Clear()
		{
			rewardWidgets.ForEach(x=> x.Clear());
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}