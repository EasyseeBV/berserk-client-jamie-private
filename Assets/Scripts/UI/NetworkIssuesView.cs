using DG.Tweening;
using RR.UI.FrameSystem;
using UnityEngine;

namespace UI
{
	public partial class NetworkIssuesView : BaseView
	{
		private Tween tween;
		private static readonly string MESSAGE = "Couldn't reach game servers.\nPlease check your internet connection.\n";
		protected override void OnAwake()
		{
			OkBtn.Subscribe(Application.Quit);
			BodyMessageTxt.SetText(MESSAGE + "Trying to reconnect in 5 sec");
		}

		protected override void OnShown()
		{
			tween = DOTween.Sequence()
				.Append(BodyMessageTxt.DOText(MESSAGE + "Trying to reconnect in 5 sec", 1f))
				.Append(BodyMessageTxt.DOText(MESSAGE + "Trying to reconnect in 4 sec", 1f))
				.Append(BodyMessageTxt.DOText(MESSAGE + "Trying to reconnect in 3 sec", 1f))
				.Append(BodyMessageTxt.DOText(MESSAGE + "Trying to reconnect in 2 sec", 1f))
				.Append(BodyMessageTxt.DOText(MESSAGE + "Trying to reconnect in 1 sec", 1f))
				.SetLoops(-1, LoopType.Restart);
		}

		protected override void OnClosed()
		{
			tween?.Kill();
		}
	}
}