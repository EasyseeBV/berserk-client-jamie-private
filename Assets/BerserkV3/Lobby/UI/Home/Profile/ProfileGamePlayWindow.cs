using BerserkV3.Lobby.UI.Home.Profile.Widgets;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile
{
	public class ProfileGamePlayWindow : UIWindowBase
	{
		[SerializeField] private ImageWidget favoriteFractionWidget;
		[SerializeField] private VulcaniteWidget favoriteVulcaniteWidget;
		[SerializeField] private StatsWidget statsWidget;
		
		public ImageWidget FavoriteFractionWidget => favoriteFractionWidget;
		public VulcaniteWidget FavoriteVulcaniteWidget => favoriteVulcaniteWidget;
		public StatsWidget StatsWidget => statsWidget;
	}
}