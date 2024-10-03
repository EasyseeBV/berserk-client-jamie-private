using System.Linq;
using System.Threading;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Duels
{
	public partial class LobbyDuelRoomView : SafeView
	{
		[SerializeField] private LobbyDuelPlayerView[] playerViews;
		
		public UniTask SetupAsync(DuelRoomItemData roomItemData, string currenUserId, CancellationToken token = default)
		{
			var imHost = roomItemData.HostId == currenUserId;
			SetActive(LeaveButton, true);
			SetActive(StartButton, imHost && roomItemData.IsReadyToPlay);
			playerViews.ForEach(x => x.Hide());
			TitleText.SetText(roomItemData.NameText);
			RoomCodeInput.SetTextWithoutNotify(roomItemData.RoomCode);

			return UniTask.WhenAll(roomItemData.Players.OrderBy(x => !x.IsHost).Select((player, i) =>
			{
				var playerView = playerViews[i];
				var task = playerView.BuildAsync(player.UserName, player.AvatarUrl, player.FrameUrl, token);
				
				if (player.UserId == currenUserId)
				{
					LeaveButton.onClick.RemoveAllListeners();
					LeaveButton.onClick.AddListener(player.OnLeaveTrigger);
					return task;
				}

				if (!imHost) 
					return task;
				
				playerView.SetKickButtonActive(true);
				playerView.SetKickAction(player.OnKickTrigger);
				StartButton.onClick.RemoveAllListeners();
				StartButton.onClick.AddListener(player.OnStartTrigger);
				return task;
			})).AttachExternalCancellation(token);
		}
		
		public void Clear()
		{
			StartButton.onClick.RemoveAllListeners();
			LeaveButton.onClick.RemoveAllListeners();
			playerViews.ForEach(x => x.Clear());
		}

		private void OnDestroy()
		{
			Clear();
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}
	}
}