using System;
using System.Collections.Generic;

namespace BerserkV3.Lobby.UI.Store.Models
{
	public class StoreTabViewModel
	{
		public string Id { get; }
		public string Title { get; }
		public string Description { get; }
		public bool IsCategory { get; }
		public bool IsComingSoon { get; }
		public bool IsFeatured { get; }
		

		public StoreTabViewModel(string id, string title, string description, bool isCategory, bool isComingSoon, bool isFeatured)
		{
			Id = id;
			Title = title;
			Description = description;
			IsCategory = isCategory;
			IsComingSoon = isComingSoon;
			IsFeatured = isFeatured;
		}

		public override int GetHashCode() =>
			HashCode.Combine(Id, Title, Description);

		public override bool Equals(object obj) =>
			Equals(obj as StoreTabViewModel) || Equals(obj as string);

		public static bool operator ==(StoreTabViewModel left, StoreTabViewModel right) =>
			EqualityComparer<StoreTabViewModel>.Default.Equals(left, right);

		public static bool operator !=(StoreTabViewModel left, StoreTabViewModel right) =>
			!(left == right);

		private bool Equals(StoreTabViewModel other) =>
			other is not null &&
			Id == other.Id &&
			Title == other.Title &&
			Description == other.Description;

		private bool Equals(string otherId) =>
			!string.IsNullOrEmpty(otherId) && Id == otherId;
	}
}