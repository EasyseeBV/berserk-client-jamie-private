using System;
using RR.UIService;
using RR.UIService.FullFade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Store
{
	public class ConfirmPurchaseWindow : UIWindowBase, IFullFadeTarget
	{
		private const string BODY_TEXT = "YOU ARE ABOUT TO BUY <color={0}>\"{1}\"</color> WITH"; // 0 for color tag (<color=#005500>TEXT </color>) placement, 1 for the names
		private const string COLOR_CODE = "#F28F18";

		[SerializeField] private Button confirmButton;
		[SerializeField] private Button cancelButton;
		[SerializeField] private Button closeButton;
		[SerializeField] private TMP_Text priceLabel;
		[SerializeField] protected TMP_Text bodyTextLabel;

		protected override void OnDisposed()
		{
			Clear();
			base.OnDisposed();
		}
		public override void Hidden()
		{
			Clear();
			base.Hidden();
		}

		public virtual void SetBodyText(string text)
		{
			bodyTextLabel.text = string.Format(BODY_TEXT, COLOR_CODE, text);
		}

		public void SetPrice(string price)
		{
			priceLabel.text = price;
		}

		public void SetCancelAction(Action onPress)
		{
			if (cancelButton)
				cancelButton.onClick.AddListener(() => onPress?.Invoke());
		}

		public void SetCloseAction(Action onPress)
		{
			if (closeButton)
				closeButton.onClick.AddListener(() => onPress?.Invoke());
		}

		public void SetConfirmAction(Action onPress)
		{
			if (confirmButton)
				confirmButton.onClick.AddListener(() => onPress?.Invoke());
		}

		private void Clear()
		{
			if (cancelButton)
				cancelButton.onClick.RemoveAllListeners();

			if (confirmButton)
				confirmButton.onClick.RemoveAllListeners();

			if (closeButton)
				closeButton.onClick.RemoveAllListeners();
		}

		#region IFullFadeTarget implementation

		public Color? FadeColor => null;

		public void OnFadeClick()
		{
		}

		#endregion
	}
}