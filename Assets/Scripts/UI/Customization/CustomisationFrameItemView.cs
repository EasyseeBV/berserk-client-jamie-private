using System.Linq;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Lobby.Decks;
using BerserkV3.Startup.Authorization;
using UnityEngine;

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
			Debug.LogError("This old class replace with new one");
			/*var ownedHero = User.OwnedVulcanites.FirstOrDefault(x => x.Id == DeckApplicationAdapter.Application.Current?.OwnedVulcaniteId)
				?? User.OwnedVulcanites.FirstOrDefault(x=> x.IsValid());
			return GameDataBaseAdapter.Instance.GetHero(ownedHero?.VulcaniteId)?.ArtUrl;*/
			return "";
		}
	}
}