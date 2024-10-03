using System;
using Berserk.Shared.Data.Customisation;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationItemFactory : ICustomisationItemFactory
	{
		public CustomisationItem Create(CustomisationData data, bool isEquipped)
		{
			switch (data.Type)
			{
				case CustomisationType.AvatarFrame:
				case CustomisationType.Gameboard:
				case CustomisationType.Background:
				case CustomisationType.CardBack:
				case CustomisationType.LobbyMusic:
				case CustomisationType.BattleMusic:
					return new CustomisationItem(data.Id, data.PreviewUrl, data.AssetData, data.Title, data.Type, isEquipped, data.IsDefault);

				case CustomisationType.Emotions:
					return new EmotionCustomisationItem(data.Id, data.PreviewUrl, data.AssetData, data.Title, data.Type, isEquipped, data.IsDefault);

				case CustomisationType.None:
				default: throw new NotImplementedException($"CustomizationItemType : {data.Type} unsupported");
			}
		}
	}
}