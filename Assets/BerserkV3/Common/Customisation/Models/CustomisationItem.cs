using System;
using Berserk.Shared.Data.Customisation;
using Newtonsoft.Json;
using RR.Core.Extensions;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationItem : IEquatable<CustomisationItem>
	{
		public string Id { get; }
		public string PreviewURL { get; }
		public virtual AssetData AssetData { get; protected set; }
		public CustomisationType CustomisationType { get; }
		public bool IsEquipped { get; protected set; }
		public bool IsDefault { get; protected set; }
		public string Title { get; protected set; }

		public event Action<string> OnEquipped;

		public event Action<string> OnUnequipped;

		public CustomisationItem(
			string id, 
			string previewURL, 
			string assetData, 
			string title,
			CustomisationType customisationType, 
			bool isEquipped,
			bool isDefault)
		{
			if (string.IsNullOrEmpty(id))
				throw new NullReferenceException();

			if (customisationType == CustomisationType.None)
				throw new ArgumentException($"{nameof(CustomisationItem)}, Id = {id}; {nameof(CustomisationType)} = None");

			if (string.IsNullOrEmpty(assetData))
				throw new NullReferenceException();

			if (!TryParseAssetData(assetData))
				throw new NullReferenceException();

			Id = id;
			PreviewURL = previewURL;
			CustomisationType = customisationType;
			IsEquipped = isEquipped;
			IsDefault = isDefault;
			Title = title;
		}

		protected virtual bool TryParseAssetData(string assetData)
		{
			AssetData = JsonConvert.DeserializeObject<AssetData>(assetData);
			return AssetData != null;
		}

		public void Equip()
		{
			IsEquipped = true;
			OnEquippedTrigger();
		}

		public void Unequip()
		{
			IsEquipped = false;
			OnUnequippedTrigger();
		}

		public void Clear()
		{
			OnEquipped = null;
			OnUnequipped = null;
		}

		public bool Equals(CustomisationItem other)
		{
			return Id.Same(other?.Id);
		}

		protected void OnEquippedTrigger()
		{
			OnEquipped?.Invoke(Id);
		}

		protected void OnUnequippedTrigger()
		{
			OnUnequipped?.Invoke(Id);
		}
	}
}