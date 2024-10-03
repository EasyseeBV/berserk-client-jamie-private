using System;

namespace Berserk.Shared.Data.Customisation
{
	[Serializable]
	public class CustomisationItemDto : IEquatable<CustomisationItemDto>
	{
		public string Id { get; set; }
		public CustomisationType Type { get; set; }

		public bool Equals(CustomisationItemDto other)
		{
			if (ReferenceEquals(null, other)) 
				return false;
			
			return Id == other.Id && Type == other.Type;
		}

		public override bool Equals(object obj)
		{
			if (obj is not CustomisationItemDto dto) 
				return false;
			
			return Equals(dto);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id, Type);
		}
	}
}