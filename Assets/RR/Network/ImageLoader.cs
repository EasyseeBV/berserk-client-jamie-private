using DG.Tweening;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RR.Network
{
	[RequireComponent(typeof(RawImage))]
	[Obsolete("Switch everything to urls with picLoader and remove this")]
	public class ImageLoader : PNGLoader
	{
		public RawImage image;

		// pulsating animation
		private float pulseAnimTime;

		[Header("Image Loader")]
		[Tooltip("An optional image to show while loading")]
		[SerializeField]
		private Sprite placeholder = null;
		[Tooltip("An optional image to show when load fails")]
		[SerializeField]
		private Sprite errorSprite = null;

		[Tooltip("The placeholder image alpha (will be multiplied by image alpha")]
		public float placeholderAlpha = 1.0f;

		private Tween fadeInOutTween;

		protected override void Awake()
		{
			base.Awake();
			image = image?.Value() ?? GetComponentInChildren<RawImage>();
		}

		private void Start()
		{
			if (loadOnStart)
				Load(_startLoadURL);
		}

		private Tween InitAnimation()
		{
			if (fadeInOutTween != null)
				return fadeInOutTween;

			fadeInOutTween = DOTween.ToAlpha(() => image.color, x => image.color = x, 0, 1)
				.SetLoops(-1, LoopType.Yoyo)
				.SetEase(Ease.Linear);
			return fadeInOutTween;
		}

		/// <summary>
		/// Loads the image from url
		/// </summary>
		public void Load(string imageUrl, Action<string> onComplete = null, Action<string> onFail = null)
		{
			// set sprite as placeholder when loading
			ResetImage();

			// RRLogger.( "Start Loading, " + imageUrl);
			if (string.IsNullOrEmpty(imageUrl))
			{
				RRLogger.Warning("Image URL is null");
				onFail?.Invoke(imageUrl);
				return;
			}

			// set pulsating anim
			InitAnimation().Play();

			LoadImage(imageUrl, t =>
				 {
					// getting errors that image loader has been destroyed but callback is still returning
					if (!this)
						 return;

					 fadeInOutTween.Rewind();
					 SetAlpha(1);

					 if (t)
						 TextureLoadedCallback(t);
					 else
						 RRLogger.Error("ImageLoader texture is null");

					 onComplete?.Invoke(imageUrl);
				 }, e =>
				 {
					 RRLogger.Error(e);

					 fadeInOutTween.Rewind();
					 SetAlpha(1);

					 if (errorSprite != null)
						 SetTexture(errorSprite.texture);
					 else
						 ResetImage();

					 onFail?.Invoke(imageUrl);
				 });
		}

		private void TextureLoadedCallback(Texture2D texture)
		{
			if (!this)
				return;

			fadeInOutTween?.Rewind();
			SetAlpha(1);
			SetTexture(texture ? texture : placeholder.texture);
		}

		/// <summary>
		/// Sets image back to placeholder
		/// </summary>
		public void ResetImage()
		{
			if (placeholder)
				SetTexture(placeholder.texture, placeholderAlpha);
		}

		private void SetAlpha(float a)
		{
			var c = image.color;
			c.a = a;
			image.color = c;
		}

		public void SetTexture(Texture texture, float newAlpha = 1, bool fade = false)
		{
			// set sprite
			if (texture && image)
			{
				image.texture = texture;

				var c = image.color;
				c.a = newAlpha;
				image.color = c;

				ResizeImage(texture);
			}

			// set alpha
			if (fade)
			{
				DOTween.ToAlpha(() => image.color, x => image.color = x, 1, .75f);
			}
			else
				SetAlpha(newAlpha);
		}

		protected void ResizeImage(Texture texture)
		{
			var aspect = (float)texture.height / texture.width;
			var rectTransform = (RectTransform)image.transform;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x * aspect);
		}

#if UNITY_EDITOR
		private void Reset()
		{
			image = this.GetOrAddComponent<RawImage>();
		}
#endif
	}
}