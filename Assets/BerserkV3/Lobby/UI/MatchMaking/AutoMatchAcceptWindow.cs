using System;
using RR.UIService;
using RR.UIService.FullFade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.MatchMaking
{
	public class AutoMatchAcceptWindow : UIWindowBase, IFullFadeTarget
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI MessageText;
		[SerializeField] protected GameObject FillLayout;
		[SerializeField] protected GameObject ButtonsLayout;
		[SerializeField] protected Image FillImage;
		[SerializeField] protected Button AcceptButton;
		[SerializeField] protected Button DeclineButton;

		public override void Hidden()
		{
			Clear();
			base.Hidden();
		}

		protected override void OnDisposed()
		{
			Clear();
			base.OnDisposed();
		}

		public void SetAcceptAction(Action<bool> value)
		{
			if (DeclineButton)
				DeclineButton.onClick.AddListener(() => value?.Invoke(false));

			if (AcceptButton)
				AcceptButton.onClick.AddListener(() => value?.Invoke(true));
		}

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetMessageText(string value)
		{
			if (MessageText)
				MessageText.SetText(value);
		}

		public void SetFill(float value01)
		{
			if (FillImage)
				FillImage.fillAmount = value01;
		}

		public void SetMessgeVisible(bool value)
		{
			if (MessageText)
				MessageText.gameObject.SetActive(value);
		}

		public void SetFillVisible(bool value)
		{
			if (FillLayout)
				FillLayout.SetActive(value);
		}
		
		public void SetAcceptButtonVisible(bool value)
		{
			if (AcceptButton)
				AcceptButton.gameObject.SetActive(value);
			
			var allButtonsHided = !value && (!DeclineButton || !DeclineButton.gameObject.activeSelf);
			SetButtonsVisible(!allButtonsHided);
		}

		public void SetDeclineButtonVisible(bool value)
		{
			if (DeclineButton)
				DeclineButton.gameObject.SetActive(value);

			var allButtonsHided = !value && (!AcceptButton || !AcceptButton.gameObject.activeSelf);
			SetButtonsVisible(!allButtonsHided);
		}

		public void SetButtonsVisible(bool value)
		{
			if (ButtonsLayout)
				ButtonsLayout.SetActive(value);
		}

		public void Clear()
		{
			if (AcceptButton)
				AcceptButton.onClick.RemoveAllListeners();

			if (DeclineButton)
				DeclineButton.onClick.RemoveAllListeners();
		}

		public Color? FadeColor => null;
		public void OnFadeClick() {}
	}
}