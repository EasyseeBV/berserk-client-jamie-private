using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public abstract class BaseStatisticView : BaseView
	{
		[SerializeField] private HorizontalOrVerticalLayoutGroup layoutGroup;
		[SerializeField] private AspectRatioFitter imageRatioFitter;
		public abstract RawImage RawImage { get; }

		public abstract RawImage RawImageMask { get; }

		public abstract Image BackgroundUnderWhole { get; }

		public abstract Image BackgroundUnderText { get; }

		public abstract TextMeshProUGUI StatTextKey { get; }

		public abstract TextMeshProUGUI StatTextValue { get; }

		public abstract GameObject RawImageLayout { get; }

		public void ReverseArrangement(bool value)
		{
			layoutGroup.reverseArrangement = value;
		}

		public void SetActiveTextKey(bool value)
		{
			StatTextKey.gameObject.SetActive(value);
		}

		public void SetActiveTextValue(bool value)
		{
			StatTextValue.gameObject.SetActive(value);
		}

		public void SetActiveImage(bool value)
		{
			RawImageLayout.SetActive(value);
		}

		public void SetActiveImageMask(bool value)
		{
			RawImageMask.enabled = value;
		}

		public void SetActiveTextBackground(bool value)
		{
			BackgroundUnderText.gameObject.SetActive(value);
		}

		public void SetActiveBackground(bool value)
		{
			BackgroundUnderWhole.gameObject.SetActive(value);
		}

		public void SetTextValue(string value)
		{
			StatTextValue.SetText(value ?? string.Empty);
		}

		public void SetTextKey(string value)
		{
			StatTextKey.SetText(value ?? string.Empty);
		}

		public void SetActive(bool value)
		{
			if (!gameObject)
				return;
			
			gameObject.SetActive(value);
			SetVisibleState(value ? VisibleState.Visible : VisibleState.Hidden);
		}
		
		public void SetImageAspectRatio(float value = -1)
		{
			if(!RawImage.texture)
				return;
			
			if (value < 0)
			{
				value = (float) RawImage.texture.width / RawImage.texture.height;
			}

			imageRatioFitter.aspectRatio = value;
		}
	}
}