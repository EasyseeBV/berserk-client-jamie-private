using Berserk.Shared.Data.Game;
using BerserkV3.Lobby.UI.Home.Profile.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace BerserkV3.Lobby.UI.General.Profile.MatchLog
{
	public class MatchLogRowWidget : MonoBehaviour
	{
		[SerializeField] private TMP_Text idText;
		[SerializeField] private TMP_Text matchIdText;
		[SerializeField] private TMP_Text matchTypeText;
		[SerializeField] private TMP_Text mmrText;
		[SerializeField] private DeckWidget deckWidget;
		[SerializeField] private VulcaniteWidget vulcaniteWidget;
		[SerializeField] private TMP_Text matchDurationText;
		[SerializeField] private TMP_Text statusText;
		
		public void Init(
			string id,
			string matchId, 
			string matchType, 
			string mmr, 
			string vulcanite, 
			string borderId, 
			CardData[] deck, 
			string matchDuration, 
			string status,
			Color statusColor)
		{
			idText.text = id;
			matchIdText.text = matchId;
			matchTypeText.text = matchType;
			mmrText.text = mmr;
			deckWidget.InitAsync(deck);
			vulcaniteWidget.InitAsync(vulcanite, borderId);
			matchDurationText.text = matchDuration;
			statusText.text = status;
			statusText.color = statusColor;
		}
	}
}