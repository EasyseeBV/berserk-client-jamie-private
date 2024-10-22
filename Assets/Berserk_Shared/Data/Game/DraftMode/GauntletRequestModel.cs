// using System;
// using Berserk.Shared.Data.Enums;
//
// namespace Berserk.Shared.Data.Game.DraftMode
// {
// 	public class DraftModeRequestModel : IEquatable<DraftModeRequestModel> //TODO DraftModeRequestEntity
// 	{
// 		public string DraftModeRequestId { get; set; }
// 		public Rarity Rarity { get; set; }
// 		public bool IsExpired { get; set; }
// 		public bool IsOpponentLeaveQueue { get; set; }
//
// 		public override bool Equals(object obj)
// 		{
// 			return obj is DraftModeRequestModel DraftModeRequestModel && Equals(DraftModeRequestModel);
// 		}
//
// 		public bool Equals(DraftModeRequestModel other)
// 		{
// 			return other != null
// 			       && DraftModeRequestId == other.DraftModeRequestId 
// 			       && Rarity == other.Rarity 
// 			       && IsExpired == other.IsExpired 
// 			       && IsOpponentLeaveQueue == other.IsOpponentLeaveQueue;
// 		}
//
// 		public override int GetHashCode()
// 		{
// 			return HashCode.Combine(DraftModeRequestId, Rarity, IsExpired, IsOpponentLeaveQueue);
// 		}
// 	}
// }