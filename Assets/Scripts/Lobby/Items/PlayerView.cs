using System;
using System.Linq;
using BerserkV3.Common.DataBase;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Items
{
	[Obsolete]
	//TODO check and remove
	public class PlayerView : MonoBehaviour
	{
		[SerializeField] private RawImage avatarImage;
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private TextMeshProUGUI descriptionText;
		[SerializeField] private int maxLengthOfName = -1;
		
		public void SetArtFromOwnedVulcaniteId(string id)
		{
			//var ownedHero = User.OwnedVulcanites.FirstOrDefault(x => x.Id == id);
			//SetArtFromVulcaniteId(ownedHero?.VulcaniteId);
		}
		
		public void SetArtFromVulcaniteId(string id)
		{
			SetArt(GameDataBaseAdapter.Instance.GetHero(id)?.ArtUrl);
		}

		public void SetArt(string artId)
		{
			if (!string.IsNullOrEmpty(artId))
				avatarImage.LoadResourceAsync(artId).Forget();
		}

		public void SetName(string value)
		{
			var parts = value.Split(',');
			if (nameText) 
				nameText.SetText(TrimName(parts[0].Trim()));
				
			// if (descriptionText) // TODO maybe not need
			// 	SetDescription(parts.Length > 1 ? TrimName(parts[1].Trim()) : "");
		}

		public void SetDescription(string value)
		{
			if (descriptionText)
				descriptionText.SetText(value);
		}
		
		private string TrimName(string value)
		{
			return maxLengthOfName > 0 && value.Length > maxLengthOfName
				? value.Substring(0, maxLengthOfName)
				: value;
		}
		
		private void OnDestroy()
		{
			avatarImage.ReleaseResource();
		}
	}
}