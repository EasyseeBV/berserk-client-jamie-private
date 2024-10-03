using DG.Tweening;
using System;
using Berserk.Shared.Data.Game;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RR.UI.FrameSystem;
using RR.Core.ResourceManagament;
using RR.Core.Extensions;
using HexagoneItemView = BerserkV3.Common.UIKit.HexagoneItemView;

namespace Lobby.Items
{
	[Obsolete("Use : " +nameof(HexagoneItemView))]
	public class PictureButton : BaseView
	{
		[SerializeField] private RawImage pictureImage = default;
		[SerializeField] private Image borderImage = default;

		[SerializeField] private Button button = default;
		[SerializeField] private GameObject lockObject = default;
		[SerializeField] private GameObject levelContainer = default;
		[SerializeField] private TextMeshProUGUI levelTxt = default;

		private readonly float selectedScale = 0.95f;
		private readonly float animationDuration = 0.3f;

		private float initialScale;
		private Tween selectTween;
		private bool selected;

		protected override void OnAwake()
		{
			initialScale = transform.localScale.x;
		}

		public PictureButton SetUp(HeroData vulcaniteData, int level, Action<string> onAvatarSelected)
		{
			level--;
			button.onClick.RemoveAllListeners();
			SetAsActive(vulcaniteData != null);

			if (vulcaniteData == null)
				return this;

			pictureImage.LoadResourceAsync(vulcaniteData.ArtUrl).Forget();

			SetAsAvailable(level >= 0);
			levelContainer.SetActive(level > 0);

			if (level >= 0)
			{
				button.onClick.AddListener(() => onAvatarSelected(vulcaniteData.Id));
				levelTxt.SetText(level.ToRoman());
			}

			return this;
		}

		public void SetAsSelected(bool selected)
		{
			if (this.selected == selected)
				return;

			this.selected = selected;
			var from = transform.localScale;
			var to = selected ? selectedScale : initialScale;
			
			if(selectTween != null && selectTween.IsComplete())
				selectTween.Kill();
			
			selectTween = transform
				.DOScale(to, animationDuration)
				.From(from)
				.SetEase(Ease.InOutCubic);
		}

		private void SetAsActive(bool active)
		{
			borderImage.enabled = active;
			pictureImage.enabled = active;
			lockObject.SetActive(active);
			levelContainer.SetActive(active);
		}

		private void SetAsAvailable(bool active)
		{
			lockObject.SetActive(!active);
			pictureImage.color = active ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.8f);
			if (!active) levelContainer.SetActive(false);
		}

		private void OnDestroy()
		{
			pictureImage.ReleaseResource();
		}
	}
}