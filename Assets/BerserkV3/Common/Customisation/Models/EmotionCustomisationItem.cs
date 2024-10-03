using Berserk.Shared.Data.Customisation;
using Newtonsoft.Json;

namespace BerserkV3.Generic.Customisation
{
	public class EmotionCustomisationItem : CustomisationItem
	{
		public EmotionCustomisationItem(
			string id, 
			string previewURL, 
			string assetData,
			string title,
			CustomisationType customisationType, 
			bool isEquipped, 
			bool isDefault) : base(id, previewURL, assetData, title, customisationType, isEquipped, isDefault)
		{
		}

		protected override bool TryParseAssetData(string assetData)
		{
			AssetData = JsonConvert.DeserializeObject<EmotionAssetData>(assetData);
			return AssetData != null;
		}
	}
}