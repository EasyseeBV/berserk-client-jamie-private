using Game.Entities;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Game.Effect_System.Target
{
	public class ManualTargetPickerView : MonoBehaviour
	{
		[SerializeField] private float segmentLength = 64f;

		private Camera mainCamera;
		private UILineRenderer arrowLine;
		private RectTransform arrowLineEndRect;
		private RectTransform rectTransform;
		private CanvasGroup canvasGroup;
		private IMonoEntity owner;

		private void Awake()
		{
			mainCamera = Camera.main;
			rectTransform = (RectTransform)transform;
			arrowLineEndRect = (RectTransform)transform.GetChild(0);
			arrowLine = this.GetOrAddComponent<UILineRenderer>();
			canvasGroup = this.GetOrAddComponent<CanvasGroup>();
		}

		public void SetUp(IMonoEntity owner)
		{
			this.owner = owner;
			canvasGroup.blocksRaycasts = false;
			gameObject.SetActive(true);
		}

		public void Drag()
		{
			var input = mainCamera.ScreenToWorldPoint(Input.mousePosition);
			var localPos = rectTransform.parent.InverseTransformPoint(input);
			rectTransform.localPosition = new Vector3(localPos.x, localPos.y, 0);

			var v0 = Vector2.zero;
			var v1 = (Vector2)arrowLine.transform.InverseTransformPoint(owner.RectTransform.position);
			var e = (v1 - v0).normalized * segmentLength;
			v0 += e / 2;

			var length = (int)((v1 - v0).magnitude / segmentLength);
			var angle = Mathf.Atan2(e.y, e.x) * Mathf.Rad2Deg;

			arrowLine.Points = new Vector2[length + 1];
			for (int i = 0; i <= length; i++)
			{
				arrowLine.Points[i] = v0;
				v0 += e;
			}

			arrowLineEndRect.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
			arrowLine.Rebuild(UnityEngine.UI.CanvasUpdate.PostLayout);
		}

		public void Close()
		{
			canvasGroup.blocksRaycasts = true;
			gameObject.SetActive(false);
		}
	}
}