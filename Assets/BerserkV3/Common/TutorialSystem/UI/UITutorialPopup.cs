using System;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.Common.TutorialSystem
{
	public partial class UITutorialPopup : BaseView
	{
		public RectTransform Container => Content;

		public void SetBodyText(string value)
		{
			if (BodyText)
				BodyText.SetText(value);
		}

		public void SetHeaderText(string value)
		{
			if (TitleText)
				TitleText.SetText(value);
		}

		public void SetAcceptButton(bool value, string text = null, Action clickAction = null)
		{
			if (!AcceptButton)
				return;

			if (value)
				AcceptButton.Show(noAnimation: true);

			if (!value)
				AcceptButton.Hide(noAnimation: true);

			AcceptButton.SetText(text ?? string.Empty);

			if (clickAction != null)
				AcceptButton.Subscribe(clickAction);
		}

		public void SetDeclineButton(bool value, string text = null, Action clickAction = null)
		{
			if (!DeclineButton)
				return;

			if (value)
				DeclineButton.Show(noAnimation: true);

			if (!value)
				DeclineButton.Hide(noAnimation: true);

			DeclineButton.SetText(text ?? string.Empty);

			if (clickAction != null)
				DeclineButton.Subscribe(clickAction);
		}

		public void SetBackButton(bool value, Action clickAction = null)
		{
			if (!BackButton)
				return;

			BackButton.gameObject.SetActive(value);
			BackButton.onClick.AddListener(() => clickAction?.Invoke());
		}

		public void SetBackHinder(bool value)
		{
			if (BackHinder)
				BackHinder.gameObject.SetActive(value);
		}

		public void Clear()
		{
			if (AcceptButton)
				AcceptButton.Clear();

			if (DeclineButton)
				DeclineButton.Clear();

			if (BackButton)
				BackButton.onClick.RemoveAllListeners();
		}

		public void Enable(bool value)
		{
			gameObject.SetActive(value);
			if (!value)
				Clear();
		}

		protected override void OnClosed()
		{
			Clear();
		}

		protected override void OnHidden()
		{
			Clear();
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}