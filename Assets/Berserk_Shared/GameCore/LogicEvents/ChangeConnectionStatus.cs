using System;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangeConnectionStatus : LogicEvent
	{
		public string UserId { get; set; }
		public bool IsConnected { get; set; }
		public DateTime ConnectionTimout { get; set; }

		public ChangeConnectionStatus(string userId, bool isConnected, DateTime connectionTimout)
		{
			UserId = userId;
			IsConnected = isConnected;
			ConnectionTimout = connectionTimout;
		}
	}
}