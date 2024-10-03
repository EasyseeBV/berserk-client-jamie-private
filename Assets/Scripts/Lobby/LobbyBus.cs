using RR.Core.EventLayer;
using System.Collections.Generic;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.SignalR.Enums;

namespace Lobby
{
	public class LobbyBus : EventBus
	{
		static LobbyBus()
		{
			InitFields<LobbyBus>();

			CurrentOnlineCount.HideInLog = true;
		}

		public static RREvent LeaguesRefereshRequered;
		public static State<List<LeagueModel>> Leagues;
		public static RREvent RequestRefreshLeagueStatistics;
		public static State<string> CurrentLeagueId;

		#region LaunchLobbyHandler
		public static RREvent LogOut;
		#endregion
		
		
		public static RREvent OnDeckSaved;
		public static RREvent OnDeckEditEntered;

		public static RREvent OnReconnectRequired;
		public static RREvent<string> OnReauthorizationRequired;
		
		public static RREvent OnRoomLaunched;
		public static RREvent OnRoomLeave;
		
		public static RREvent OnUserDataRefreshed;
		public static State<int> CurrentOnlineCount;
	}
}