using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace RR.UI.Predefined
{
	public partial class DialogWindow : BaseView
	{
		public DialogResult DialogResult;
		public bool Created;
		public string Message { get; private set; }
		public string OkButtonText { get; private set; }
		public string CancelButtonText { get; private set; }
		public bool CloseByTap { get; private set; }
		public string LongMessage { get; private set; }

		protected override void Start()
		{
			base.Start();

			Subscribe(OkBtn, () => { DialogResult = DialogResult.Ok; Close(); });
			Subscribe(CancelBtn, () => { DialogResult = DialogResult.Cancel; Close(); });
			Subscribe(btIgnore, () => { DialogResult = DialogResult.Ignore; Close(); });
			Subscribe(CopyToClipBtn, () =>
			 {
				 var te = new TextEditor
				 {
					 text = LongTextInputField.text
				 };
				 te.SelectAll();
				 te.Copy();
				 CopyToClipTxt.SetText("Copied!");
			 });

			Created = true;
		}

		[VisibleInGraph(false)]
		public DialogWindow Build(string message, string okText, string cancelText, bool closeByTap,
			string longMessage = null)
		{
			Message = message;
			OkButtonText = okText;
			CancelButtonText = cancelText;
			CloseByTap = closeByTap;
			LongMessage = longMessage;
			OnBuildSafe(true);
			return this;
		}

		protected override void OnBuild(bool isFirstBuild)
		{
			txMessage.SetText(Message);
			OkText.SetText(OkButtonText);
			CancelText.SetText(CancelButtonText);

			SetActive(OkBtn, OkButtonText != null);
			SetActive(CancelBtn, CancelButtonText != null);
			SetActive(btIgnore, CancelButtonText != null);
			LongTextInputField.gameObject.SetActive(!string.IsNullOrEmpty(LongMessage));
			LongTextInputField.text = LongMessage?.Ellipsis(2400) ?? string.Empty;

			LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);

			DialogResult = DialogResult.None;
		}

		public override void OnGesture(GestureInfo info)
		{
			base.OnGesture(info);

			if (info.Gesture != Gesture.Tap || !CloseByTap)
				return;

			RRLogger.Log("Closed by gesture");
			DialogResult = DialogResult.Cancel;
			info.IsHandled = true;
			Close();
		}

		protected override void OnDisable()
		{
			if (Created)
				Destroy(gameObject);
			base.OnDisable();
		}
	}
	public enum DialogResult
	{
		None, Ok, Cancel, Ignore
	}
}