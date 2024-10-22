using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Lobby.Matchmaking.Duels;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public static class DuelExtensions
	{
		public static string RemoveWhitespace(this string value)
		{
			value = value?.Replace(" ", "").Replace("\t", "");
			return string.IsNullOrWhiteSpace(value) ? null : value;
		}
		
		public static List<DuelRoomItemData> MapDuelRooms(this IEnumerable<DuelRoomModel> rooms, int playerLimit)
		{
			if (rooms == null)
				return new List<DuelRoomItemData>();
			
			return rooms
				.OrderByDescending(x => x.CreatedOn)
				.Select((roomModel, i) =>  roomModel.MapDuelRoom(playerLimit, i))
				.ToList();
		}
		
		public static DuelRoomItemData MapDuelRoom(this DuelRoomModel roomModel, int playerLimit, int order)
		{
			if (roomModel == null)
				return default;
			
			return new DuelRoomItemData
			{
				Id = roomModel.Id,
				RoomCode = roomModel.RoomCode,
				NameText = roomModel.Name,
				RoomSlotsText = $"{roomModel.Players.Count}/{playerLimit}",
				HostId = roomModel.HostId,
				IsLocked = roomModel.IsLockedByPassword,
				IsPrivate = roomModel.IsPrivate,
				IsReadyToPlay = roomModel.Players.Count == playerLimit,
				IsStarted = roomModel.StartedDateTime.HasValue,
				IsClosed = roomModel.ClosedDateTime.HasValue,
				Order = order,
				Players = roomModel.Players.Select(playerModel => new DuelPlayerItemData
				{
	  				UserId = playerModel.UserId,
	  				UserName = playerModel.UserName,
	  				AvatarUrl = playerModel.AvatarUrl,
	  				FrameUrl = playerModel.FrameUrl,
					IsHost = playerModel.UserId == roomModel.HostId
				}).ToList()
			};
		}
	}
}