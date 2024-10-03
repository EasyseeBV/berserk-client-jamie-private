using DG.Tweening;
using RR.Core;
using RR.Core.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RR.Game.StatDisplayBar
{
	public class TopDownStatBar : MonoBehaviour
	{
		[SerializeField] protected Gradient gradient;
		[SerializeField] protected Image statFill;
		[SerializeField] protected TextMeshProUGUI txtValue;

		protected RectTransform targetCanvas;
		protected Transform objectToFollow;
		protected RectTransform rc;
		protected CanvasGroup cv;

		protected Vector2 offset;
		protected Collider col;
		protected FloatStat statOnStatBar;

		protected Tween fadeTween;

		private void Awake()
		{
			rc = transform as RectTransform;
			targetCanvas = GetComponentInParent<Canvas>().transform as RectTransform;
			cv = this.GetOrAddComponent<CanvasGroup>();
		}

		private void OnDestroy()
		{
			statOnStatBar.OnChanged -= StatChanged;
			fadeTween?.Kill();
		}

		private void LateUpdate()
		{
			RepositionStatBar();
		}

        public TopDownStatBar AttachTo(Transform owner, FloatStat statOnStatBar, Vector2 offset = default, bool isVisible = true)
		{
			this.offset = offset;
			col = owner.GetComponent<Collider>();
			this.statOnStatBar = statOnStatBar;
			objectToFollow = owner;
			RepositionStatBar();
			gameObject.SetActive(true);

			statOnStatBar.OnChanged += StatChanged;
			StatChanged(statOnStatBar);

			Fade(isVisible, true);

            return this;
		}

		public virtual void SetSize(float mod = 1.25f)
		{
			if (col == null)
				return;

			var boundsSize = BoundsToScreenRect(col.bounds).size;
			boundsSize.y = rc.sizeDelta.y;
			boundsSize.x /= mod;
			rc.sizeDelta = boundsSize;
		}

		public virtual void StatChanged(float current)
		{
			var percent = statOnStatBar.PercentOfMax();

			if (percent < 0)
				percent = 0;

			statFill.fillAmount = percent;
			if (txtValue == null)
				return;

			statFill.color = gradient.Evaluate(percent);
			txtValue.SetText(statOnStatBar.ToString("####"));
		}

		public Rect BoundsToScreenRect(Bounds bounds)
		{
			var origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
			var extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));

			return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
		}

		private void RepositionStatBar()
		{
            if (objectToFollow == null)
            {
                Destroy(gameObject);
                return;
            }

			var viewportPosition = Camera.main.WorldToViewportPoint(objectToFollow.position);
			var worldObjectScreenPosition = new Vector2(
				(viewportPosition.x + offset.x) * targetCanvas.sizeDelta.x - targetCanvas.sizeDelta.x * 0.5f,
				(viewportPosition.y + offset.y) * targetCanvas.sizeDelta.y - targetCanvas.sizeDelta.y * 0.5f
			);

			rc.anchoredPosition = worldObjectScreenPosition;
		}

		public void Fade(bool @in, bool instant = false)
		{
			if (instant)
			{
				cv.alpha = @in ? 1 : 0f;
				return;
			}

			fadeTween?.Kill();
			fadeTween = cv.DOFade(@in ? 1 : 0, 1f).SetEase(Ease.OutExpo);
		}
	}
}
