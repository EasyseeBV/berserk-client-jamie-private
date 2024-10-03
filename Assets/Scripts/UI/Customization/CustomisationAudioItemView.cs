using System.Threading;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using TMPro;
using UnityEngine;

namespace UI
{
	public class CustomisationAudioItemView : CustomisationItemView
	{
		[SerializeField] protected TextMeshProUGUI TitleText;
		private CancellationTokenSource updateArt;
		
		public override async UniTask SetupAsync(CustomisationItem item, CancellationToken token = default)
		{
			updateArt?.Cancel();
			updateArt?.Dispose();
			updateArt = null;
			
			await base.SetupAsync(item, token);
			Set(TitleText, Current?.Title);
		}

		public override void Dispose()
		{
			updateArt?.Cancel();
			updateArt?.Dispose();
			updateArt = null;
			base.Dispose();
		}

		public override void Select()
		{
			base.Select();
			RefreshState();
		}

		public override void Deselect()
		{
			base.Deselect();
			RefreshState();
		}

		public override PreviewInfo GetPreviewInfo()
		{
			var info = base.GetPreviewInfo();
			info.AdditionUrl = GetMainArtUrl();
			info.PreviewUrl = Current.PreviewURL;
			info.TitleText = TitleText.text;
			return info;
		}

		protected override string GetMainArtUrl()
		{
			return Current is {IsEquipped: true}
				? "Audio_Playing_Button"
				: "Audio_ToPlay_Button";
		}

		private void RefreshState()
		{
			updateArt?.Cancel();
			updateArt?.Dispose();
			updateArt = new CancellationTokenSource();
			MainArtImage.LoadResourceAsync(GetMainArtUrl(), updateArt.Token).Forget();
		}
	}
}