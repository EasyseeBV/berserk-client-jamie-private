using System;

namespace Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching
{
	public class AutoMatchStateModel : AutoMatchBaseModel, IEquatable<AutoMatchStateModel>
	{
		public bool IsAutoMatchJoined { get; set; }
		public bool IsMarkedForAutoMatch { get; set; }
		public bool IsMatchFound { get; set; }
		public bool? IsMatchAccepted { get; set; }
		public DateTime? AcceptionTimeOut { get; set; }

		public long TimeStamp { get; set; }

		public bool Equals(AutoMatchStateModel other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return MatchMode == other.MatchMode 
			       && IsAutoMatchJoined == other.IsAutoMatchJoined 
			       && IsMarkedForAutoMatch == other.IsMarkedForAutoMatch 
			       && IsMatchFound == other.IsMatchFound 
			       && IsMatchAccepted == other.IsMatchAccepted 
			       && Nullable.Equals(AcceptionTimeOut, other.AcceptionTimeOut);
		}

		public override bool Equals(object obj)
		{
			return obj is AutoMatchStateModel other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(MatchMode, IsAutoMatchJoined, IsMarkedForAutoMatch, IsMatchFound, IsMatchAccepted, AcceptionTimeOut);
		}
	}
}