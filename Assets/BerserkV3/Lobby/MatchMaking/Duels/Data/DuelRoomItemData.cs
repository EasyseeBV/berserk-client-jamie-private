using System;
using System.Collections.Generic;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelRoomItemData
	{
		public string Id { get; set; }
		public string RoomCode { get; set; }
		public string NameText { get; set; }
		public string RoomSlotsText { get; set; }
		public string HostId { get; set; }
		public bool IsLocked { get; set; }
		public bool IsPrivate { get; set; }
		public bool IsReadyToPlay { get; set; }
		public bool IsStarted { get; set; }
		public bool IsClosed { get; set; }
		public int Order { get; set; }
		public List<DuelPlayerItemData> Players { get; set; } = new();
		
		private event Action<DuelRoomItemData> OnSelectedAction;

		public DuelRoomItemData SetSelectedAction(Action<DuelRoomItemData> value)
		{
			OnSelectedAction += item => value?.Invoke(item);
			return this;
		}
		
		public void OnClickTrigger()
		{
			OnSelectedAction?.Invoke(this);
		}
		
		public void Dispose()
		{
			Players?.ForEach(x=> x.Dispose());
			Players?.Clear();
			Players = null;
			OnSelectedAction = null;
			Id = null;
			NameText = null;
			RoomSlotsText = null;
			IsLocked = false;
		}
	}
}