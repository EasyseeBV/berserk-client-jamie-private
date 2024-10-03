using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Vulcan.Data;

namespace Vulcan.Shop.Domain
{
	public class VulcaniteEntity : IDisposable
	{
		private List<EffectInfo> effectsInfo;
		public string Id { get; }
		public int Level { get; }
		public Quadrant Quadrant { get; }
		public string VulcaniteName { get; }
		public string AvatarUri { get; }
		public bool IsOwned { get; private set; }
		public bool IsSelect { get; private set; }
		public double Price { get; }

		public event Action<bool> OnSelected;

		public VulcaniteEntity(
			string vulcaniteId,
			int level,
			Quadrant quadrant,
			string vulcaniteName,
			string vulcaniteAvatarUri,
			bool isOwned,
			double price,
			List<EffectInfo> effectsInfo)
		{
			if (string.IsNullOrEmpty(vulcaniteName) || string.IsNullOrEmpty(vulcaniteId) || string.IsNullOrEmpty(vulcaniteAvatarUri))
				throw new ArgumentNullException();

			if (level < 0 || price < 0)
				throw new ArgumentOutOfRangeException();

			Id = vulcaniteId;
			Level = level;
			Quadrant = quadrant;
			VulcaniteName = vulcaniteName;
			AvatarUri = vulcaniteAvatarUri;
			IsOwned = isOwned;
			Price = price;
			this.effectsInfo = effectsInfo;
		}

		public string GetEffectsInfoText()
		{
			var effects = "";
			effectsInfo.ForEach(info => effects = $"{effects} {info.Description.Replace("{value}", $"{info.Value}")}\n\n\t");
			return effects;
		}

		public void Select()
		{
			IsSelect = true;
			OnSelected?.Invoke(true);
		}

		public void Unselect()
		{
			IsSelect = false;
			OnSelected?.Invoke(false);
		}

		public void Purchase()
		{
			IsOwned = true;
		}

		public void Dispose()
		{
			OnSelected = null;
		}
	}
}