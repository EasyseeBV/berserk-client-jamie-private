using System;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelPlayerItemData
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string AvatarUrl { get; set; }
		public string FrameUrl { get; set; }
		public bool IsHost { get; set; }

		private event Action<DuelPlayerItemData> OnKickAction;
		private event Action<DuelPlayerItemData> OnLeaveAction;
		private event Action<DuelPlayerItemData> OnStartAction;

		public DuelPlayerItemData SetKickAction(Action<DuelPlayerItemData> value)
		{
			OnKickAction += item => value?.Invoke(item);
			return this;
		}

		public DuelPlayerItemData SetLeaveAction(Action<DuelPlayerItemData> value)
		{
			OnLeaveAction += item => value?.Invoke(item);
			return this;
		}

		public DuelPlayerItemData SetStartAction(Action<DuelPlayerItemData> value)
		{
			OnStartAction += item => value?.Invoke(item);
			return this;
		}

		public void OnKickTrigger()
		{
			OnKickAction?.Invoke(this);
		}

		public void OnLeaveTrigger()
		{
			OnLeaveAction?.Invoke(this);
		}

		public void OnStartTrigger()
		{
			OnStartAction?.Invoke(this);
		}

		public void Dispose()
		{
			OnKickAction = null;
			OnLeaveAction = null;
			OnStartAction = null;
			UserId = null;
			UserName = null;
			AvatarUrl = null;
			FrameUrl = null;
			IsHost = false;
		}
	}
}