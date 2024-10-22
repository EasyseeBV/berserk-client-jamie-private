using BerserkV3.Lobby.UI.Home.Profile.Widgets;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Profile
{
	public class ProfileGeneralWindow : UIWindowBase
	{
		[SerializeField] private ProfileWidget profileWidget;
		[SerializeField] private ImageWidget favoriteFactionWidget;
		[SerializeField] private VulcaniteWidget favoriteVulcaniteWidget;
		[SerializeField] private MmrWidget mmrWidget;
		[SerializeField] private DeckWidget deckWidget;
		
		public ProfileWidget ProfileWidget => profileWidget;
		public ImageWidget FavoriteFactionWidget => favoriteFactionWidget;
		public VulcaniteWidget FavoriteVulcaniteWidget => favoriteVulcaniteWidget;
		public MmrWidget MmrWidget => mmrWidget;
		public DeckWidget DeckWidget => deckWidget;
	}
}