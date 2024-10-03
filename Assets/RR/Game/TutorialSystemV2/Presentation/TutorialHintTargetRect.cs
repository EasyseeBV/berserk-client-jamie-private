using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Presentation
{
	public class TutorialHintTargetRect : BaseTutorialHintTarget
	{
		[Tooltip("Should scale affect to size?.")]
		[SerializeField] protected bool useScale = true;
		// from the center of rect space into world space
		public override TutorialVector3 Position => RectTransform.TransformPoint(RectTransform.rect.center);

		public override TutorialVector3 Size => RectTransform.rect.size * SizeScale.vector * (useScale ? RectTransform.localScale : Vector3.one );
		
		public RectTransform RectTransform { get; private set; }

		protected override void OnInit()
		{
			RectTransform = (RectTransform)Transform;
			base.OnInit();
		}

		protected override void OnDisposed()
		{
			RectTransform = null;
			base.OnDisposed();
		}
	}
}