using System.Linq;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Deck;

namespace UI
{
	public class CustomisationFrameItemView : CustomisationItemView
	{
		public override PreviewInfo GetPreviewInfo()
		{
			var info = base.GetPreviewInfo();
			info.AdditionUrl = GetAdditionArtUrl();
			info.PreviewMaskUrl = "Vulcanite_Mask";
			return info;
		}

		private string GetAdditionArtUrl()
		{
			var ownedHero = VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == DeckApplicationAdapter.Application.Current?.OwnedVulcaniteId)
				?? VulcaniteHandler.Owned.FirstOrDefault(x=> x.IsValid());
			return GameDataBaseAdapter.Instance.GetHero(ownedHero?.VulcaniteId)?.ArtUrl;
		}
	}
}