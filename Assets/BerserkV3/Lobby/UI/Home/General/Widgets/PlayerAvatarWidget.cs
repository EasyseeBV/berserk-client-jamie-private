using System;
using System.Threading;
using BerserkV3.Common.UIKit;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.General.Widgets
{
	public class PlayerAvatarWidget : UIViewBase
	{
		[SerializeField] private Button button;
		[SerializeField] private RawImage artMaskImage;
		[SerializeField] private RawImage artImage;
		[SerializeField] private RawImage frameImage;
		[SerializeField] private TMP_Text header1Text;
		[SerializeField] private TMP_Text header2Text;
		private bool releasePrevious;

		public async UniTask InitAsync(
			string artMaskUrl,
			string artUrl,
			string frameUrl,
			string header1,
			string header2,
			CancellationToken token = default)
		{
			await UniTask.WhenAll(
				artMaskImage.LoadResourceAsync(artMaskUrl, token, releasePrevious),
				artImage.LoadResourceAsync(artUrl, token, releasePrevious),
				frameImage.LoadResourceAsync(frameUrl, token, releasePrevious));

			if (token.IsCancellationRequested)
				return;

			releasePrevious = true;
			header1Text.SetText(header1);
			header2Text.SetText(header2);
		}

		public void SetClickAction(Action value)
		{
			if (!button)
				return;
			
			button.onClick.AddListener(() => value?.Invoke());
		}

		private void OnDestroy()
		{
			if (button)
				button.onClick.RemoveAllListeners();
			
			if (!releasePrevious)
				return;

			releasePrevious = false;
			artMaskImage.ReleaseResource();
			artImage.ReleaseResource();
			frameImage.ReleaseResource();
		}
	}
}