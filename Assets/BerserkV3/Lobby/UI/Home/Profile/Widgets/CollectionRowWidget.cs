using Berserk.Shared.Data.Game;
using BerserkV3.Lobby.UI.Home.Profile.Widgets;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.General.Profile.Collection
{
	public class CollectionRowWidget : MonoBehaviour
	{
		[SerializeField] private TMP_Text idText;
		[SerializeField] private TMP_Text mmrText;
		[SerializeField] private TMP_Text gemText;
		[SerializeField] private CardWidget cardWidget;
		[SerializeField] private TMP_Text winrateText;
		[SerializeField] private TMP_Text totalPLayedText;
		[SerializeField] private TMP_Text nameText;
		
		public void Init(
			string id,
			string mmr, 
			string winrate, 
			string totalPlayed, 
			CardData data,
			string rarity,
			string title)
		{
			idText.text = id;
			gemText.text = rarity;
			nameText.text = title;
			mmrText.text = mmr;
			cardWidget.InitAsync(data);
			winrateText.text = winrate;
			totalPLayedText.text = totalPlayed;
		}
	}
}