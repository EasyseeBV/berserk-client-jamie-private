using BerserkV3.GameCore.TargetSystem.Abstraction;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace BerserkV3.GameCore.TargetSystem
{
	public class TargetPickerView : MonoBehaviour, ITargetPickerView
	{
		[SerializeField] private float segmentLength = 64f;
		[SerializeField] private RectTransform selfContainer;
		[SerializeField] private RectTransform arrowRect;
		[SerializeField] private UILineRenderer line;
		[SerializeField] private Image arrowBodyPlaceHolder;
		[SerializeField] private Image arrowHeadImage;
		
		private Transform fromTarget;
		private bool releasePrevious;
		public bool Enabled { get; private set; }
		
		private void Awake()
		{
			gameObject.SetActive(Enabled = false);
			EnsureLineInitialized();
			line.enabled = false;
			arrowHeadImage.enabled = false;
			arrowBodyPlaceHolder.LoadResourceAsync("Arrow_Body", relesePrevious:releasePrevious)
				.ContinueWith(() => line.sprite = arrowBodyPlaceHolder.sprite != null ? arrowBodyPlaceHolder.sprite : arrowHeadImage.sprite)
				.Forget();
			
			arrowHeadImage.LoadResourceAsync("Arrow_Head", relesePrevious:releasePrevious)
				.ContinueWith(() =>
				{
					if (!line.sprite)
						line.sprite = arrowHeadImage.sprite;
				})
				.Forget();
			releasePrevious = true;
		}

		public void Enable(Transform fromTaget)
		{
			fromTarget = fromTaget;
			EnsureLineInitialized();
			transform.SetAsLastSibling();
			line.enabled = true;
			arrowHeadImage.enabled = true;
			gameObject.SetActive(Enabled = true);
			RRLogger.Log("Arrow init drag");
		}

		public void Disable()
		{
			fromTarget = null;
			line.enabled = false;
			arrowHeadImage.enabled = false;
			gameObject.SetActive(Enabled = false);
			RRLogger.Log("Arrow end drag");
		}
		
		public void Drag(Vector3 position)
		{
			if (!Enabled || !fromTarget.Value())
				return;

			EnsureLineInitialized();

			var localPos = selfContainer.parent.InverseTransformPoint(position);
			selfContainer.localPosition = new Vector3(localPos.x, localPos.y, 0);

			var v0 = Vector2.zero;
			var v1 = (Vector2)line.transform.InverseTransformPoint(fromTarget.position);
			var e = (v1 - v0).normalized * segmentLength;
			v0 += e / 2;

			var length = Mathf.Max(1, (int)((v1 - v0).magnitude / segmentLength));
			var angle = Mathf.Atan2(e.y, e.x) * Mathf.Rad2Deg;

			line.Points = new Vector2[length + 1];
			for (var i = 0; i <= length; i++)
			{
				line.Points[i] = v0;
				v0 += e;
			}

			arrowRect.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
			line.Rebuild(CanvasUpdate.PostLayout);
		}

		private void EnsureLineInitialized()
		{
			if (line == null)
				return;

			if (line.Points != null && line.Points.Length > 0)
				return;

			line.Points = new[]
			{
				Vector2.zero,
				new Vector2(segmentLength, 0)
			};
		}

		private void OnDestroy()
		{
			if (releasePrevious)
			{
				arrowHeadImage.ReleaseResource();
				arrowBodyPlaceHolder.ReleaseResource();
			}

			releasePrevious = false;
		}
	}
}
