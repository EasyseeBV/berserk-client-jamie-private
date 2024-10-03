using System;
using RR.UI.FrameSystem;

namespace BerserkV3.Common.TutorialSystem
{
	public partial class UITutorialWelcome : BaseView
	{
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

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetBodyText(string value)
		{
			if (BodyText)
				BodyText.SetText(value);
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

		public void Clear()
		{
			if (AcceptButton)
				AcceptButton.Clear();

			if (DeclineButton)
				DeclineButton.Clear();
		}
	}
}