using BerserkV3.Lobby.MatchMaking.Duels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Duels
{
	public class LobbyDuelItemView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI playersCountText;
		[SerializeField] private TextMeshProUGUI roomNameText;
		[SerializeField] private GameObject lockImage;
		[SerializeField] private Button joinButton;
		private Transform selfContainer;
		private GameObject selfObject;
		public string Id { get; private set; }

		public void Setup(DuelRoomItemData value)
		{
			if (!selfContainer)
				selfContainer = transform;
			
			Id = value.Id;
			joinButton.onClick.RemoveAllListeners();
			joinButton.onClick.AddListener(value.OnClickTrigger);
			roomNameText.SetText(value.NameText);
			lockImage.gameObject.SetActive(value.IsLocked);
			playersCountText.SetText(value.RoomSlotsText);
			selfContainer.SetSiblingIndex(value.Order);
			SetActive(!value.IsPrivate);
		}

		public void SetActive(bool value)
		{
			if (!selfObject)
				selfObject = gameObject;
			
			gameObject.SetActive(value);
		}

		public void Clear()
		{
			joinButton.onClick.RemoveAllListeners();
		}
	}
}