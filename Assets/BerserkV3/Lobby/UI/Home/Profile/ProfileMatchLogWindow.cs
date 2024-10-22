using BerserkV3.Lobby.UI.General.Profile.MatchLog;
using RR.Core.Extensions;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile
{
	public class ProfileMatchLogWindow : UIWindowBase
	{
		[SerializeField] private MatchLogRowWidget profileMatchLogRowPrefab;
		[SerializeField] private Transform contentParent;
		
		public MatchLogRowWidget CreateRow()
		{
			return Instantiate(profileMatchLogRowPrefab, contentParent);
		}
		
		public void Clear()
		{
			contentParent.DestroyChildren();
		}
	}
}