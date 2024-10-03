using UnityEngine;

namespace RR.Game.TutorialSystem.Presentation.UI
{
	public enum CloseMode
	{
		DontClose = -1,
		None,
		Click,
		Enter,
		Exit,
		Drop,
		DoubleClick
	}

	public class TransformHintTarget : HintTarget
	{
		[SerializeField] protected CloseMode CloseMode;

		public override bool IsClickToCloseHint => CloseMode != CloseMode.None;

		private void OnMouseDown()
		{
			if (CloseMode != CloseMode.Click)
				return;

			OnTargetClick();
		}

		private void OnMouseExit()
		{
			if (CloseMode != CloseMode.Exit)
				return;

			OnTargetClick();
		}

		private void OnMouseEnter()
		{
			if (CloseMode != CloseMode.Enter)
				return;

			OnTargetClick();
		}
	}
}