using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Presentation
{
	public class TutorialHintTarget : BaseTutorialHintTarget
	{
		protected enum HintTargetMode
		{
			DontClose = -1,
			None,
			Click,
			Enter,
			Exit,
			Drop,
			DoubleClick
		}

		[SerializeField] protected HintTargetMode closeMode = HintTargetMode.None;

		public override TutorialVector3 Position => Transform.position;
		public override TutorialVector3 Size => Transform.localScale * SizeScale;

		protected virtual void OnMouseDown()
		{
			if (!Initialized || closeMode != HintTargetMode.Click)
				return;

			HandleClick();
		}

		protected virtual void OnMouseExit()
		{
			if (!Initialized || closeMode != HintTargetMode.Exit)
				return;

			HandleClick();
		}

		protected virtual void OnMouseEnter()
		{
			if (!Initialized || closeMode != HintTargetMode.Enter)
				return;

			HandleClick();
		}
	}
}