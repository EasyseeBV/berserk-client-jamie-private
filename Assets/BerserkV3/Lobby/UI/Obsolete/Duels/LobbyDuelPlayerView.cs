using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Duels
{
	public class LobbyDuelPlayerView : MonoBehaviour
	{
		[SerializeField] private Button kickButton;
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private RawImage frameImage;
		[SerializeField] private RawImage avatarImage;
		private bool releasePrevious;

		public async UniTask BuildAsync(string userName, string heroArtUrl, string frameArtUrl, CancellationToken token = default)
		{
			userName = userName.Ellipsis(16); // Clip length of user name.
			nameText.SetText(userName);
			var canLoadImages = !string.IsNullOrEmpty(frameArtUrl) && !string.IsNullOrEmpty(heroArtUrl);
			
			if (releasePrevious) // only when any art loaded
			{
				frameImage.gameObject.SetActive(canLoadImages);
				avatarImage.gameObject.SetActive(canLoadImages);
			}
			
			if (!canLoadImages)
				return;
			
			await UniTask.WhenAll(
					frameImage.LoadResourceAsync(frameArtUrl, token, releasePrevious), 
					avatarImage.LoadResourceAsync(heroArtUrl, token, releasePrevious))
				.AttachExternalCancellation(token);
			
			releasePrevious = true;
			frameImage.gameObject.SetActive(true);
			avatarImage.gameObject.SetActive(true);
		}

		public void SetKickButtonActive(bool value)
		{
			if (kickButton)
				kickButton.gameObject.SetActive(value);
		}

		public void Hide()
		{
			if (nameText)
				nameText.SetText("");
			
			SetKickButtonActive(false);
			
			if (frameImage)
				frameImage.gameObject.SetActive(false);
			
			if (avatarImage)
				avatarImage.gameObject.SetActive(false);
			
			if (kickButton)
				kickButton.onClick.RemoveAllListeners();
		}
		
		public void SetKickAction(Action value)
		{
			if (!kickButton)
				return;
			
			kickButton.onClick.RemoveAllListeners();
			kickButton.onClick.AddListener(() => value?.Invoke());
		}

		public void Clear()
		{
			Hide();
			
			if (releasePrevious)
			{
				frameImage.ReleaseResource();
				avatarImage.ReleaseResource();
			}

			releasePrevious = false;
			if (kickButton)
				kickButton.onClick.RemoveAllListeners();
		}
	}
}