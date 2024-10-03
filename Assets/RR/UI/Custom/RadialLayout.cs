using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace RR.UI.Custom
{
	public class RadialLayout : LayoutGroup
	{
		public float Distance;

		[Range(0f, 360f)]
		public float MinAngle, MaxAngle, StartAngle;

		private readonly Vector2 pivot = new Vector2(0.5f, 0.5f);

		protected override void OnEnable()
		{
			base.OnEnable(); 
			CalculateRadial();
		}

		public override void SetLayoutHorizontal() { }
		public override void SetLayoutVertical() { }

		public override void CalculateLayoutInputVertical() => CalculateRadial();
		public override void CalculateLayoutInputHorizontal() => CalculateRadial();

		private void CalculateRadial()
		{
			m_Tracker.Clear();
			if (transform.childCount == 0)
				return;

			var offsetAngle = (MaxAngle - MinAngle) / transform.childCount;
			var angle = StartAngle;

			foreach (RectTransform child in transform)
			{
				if (child.GetComponent<LayoutElement>().Value()?.ignoreLayout == true)
					continue;

				m_Tracker.Add(this, child,
					DrivenTransformProperties.Anchors |
					DrivenTransformProperties.AnchoredPosition |
					DrivenTransformProperties.Pivot);

				var pos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
				child.localPosition = pos * Distance;
				child.anchorMin = child.anchorMax = child.pivot = pivot;

				angle += offsetAngle;
			}
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			CalculateRadial();
		}
#endif
	}
}
