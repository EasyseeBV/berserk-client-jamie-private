using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine.UI;

namespace BerserkV3.Common.TutorialSystem
{

	public static class CanvasTransitionFactor
	{
		public static T SetTransitionFactorSize<T>(this T hint) where T : ITutorialHintTarget
		{

			if (hint == null || !hint.Transform)
				return hint;

			var hintScaler = hint.Transform.GetComponentInParent<CanvasScaler>();
			if (!hintScaler)
				return hint;

			if (!UITutorialUnmask.Instance)
				return hint;

			var targetScaler = UITutorialUnmask.Instance.GetComponentInParent<CanvasScaler>();
			if (!targetScaler)
				return hint;

			hint.SizeScale = new TutorialVector3
			{
				x = targetScaler.referenceResolution.x / hintScaler.referenceResolution.x,
				y = targetScaler.referenceResolution.y / hintScaler.referenceResolution.y
			};
			
			return hint;
		}
	}

}