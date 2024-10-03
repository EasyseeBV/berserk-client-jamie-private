using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using RR.Core.Extensions;
using RR.Game.TutorialSystem.Domain;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace RR.Game.TutorialSystem.Presentation.UI
{
	public partial class TutorHintView : BaseView
	{
		private RectTransform targetCanvasRectTransform;
		private Camera targetCamera;

		private List<Image> unmaskPool = new();

		public Button BackgroundButton => TutorHider;
		public Button NextButton => AcceptButton;
		public RectTransform HintDialog => TutorHintBG;

		private TutorHintEntity tutorHintEntity;
		private HintTarget[] hintTargets;

		protected override void OnAwake()
		{
			targetCanvasRectTransform = GetComponentInParent<Canvas>().transform as RectTransform;
			BackgroundButton.interactable = false;
			TutorHintText.autoSizeTextContainer = true;
			unmaskPool.Add(Unmask);
		}

		private void Update()
		{
			if (!gameObject.activeSelf)
				return;

			if (!tutorHintEntity.IsAttach)
				return;

			if (!tutorHintEntity.RequiredUnmask)
				return;

			if (hintTargets == null || hintTargets.Length == 0)
				return;

			Reposition();
		}

		public void SetUpUnmasks(TutorHintEntity hint, params HintTarget[] hintTargets)
		{
			unmaskPool.ForEach((unmask, i) => unmask.gameObject.SetActive(hint.RequiredUnmask
			                                                              && i < hintTargets.Length));

			tutorHintEntity = hint;
			if (!hint.RequiredUnmask)
				return;

			while (hintTargets.Length > unmaskPool.Count)
			{
				var newUnmask = Instantiate(Unmask, Unmask.transform.parent);
				var unmaskIndex = Unmask.transform.GetSiblingIndex();
				newUnmask.transform.SetSiblingIndex(unmaskIndex + 1);
				unmaskPool.Add(newUnmask);
				var filter = TutorHider.gameObject.AddComponent<UnmaskRaycastFilter>();
				filter.targetUnmask = newUnmask.GetComponent<Unmask>();
			}

			this.hintTargets = hintTargets;

			var unmaskCount = tutorHintEntity.UnmaskHints.Count;
			if (unmaskCount > 1
			    && tutorHintEntity.UnmaskHints.Count != hintTargets.Length)
				throw new ArgumentOutOfRangeException(
					$"The number of targets and masks do not match. Masks: {tutorHintEntity.UnmaskHints.Count}, Targets: {hintTargets.Length} by hint {tutorHintEntity.Id}");

			Resize();
			Reposition();
		}

		public void SetUpHintPopup(TutorHintEntity hint)
		{
			var requiredPopup = !string.IsNullOrEmpty(hint.Text);
			TutorHintBG.gameObject.SetActive(requiredPopup);
			TutorHider.gameObject.SetActive(hint.IsRequiredBackHider);

			if (!requiredPopup)
				return;

			TutorHintBG.anchoredPosition = hint.PopupPosition;

			var hasTitle = !string.IsNullOrEmpty(hint.Title);
			TutorHintTitle.gameObject.SetActive(hasTitle);

			TutorHintTitle.SetText(hint.Title);
			TutorHintText.SetText(hint.Text);

			ButtonsBar.Value()?.gameObject.SetActive(hint.IsNextButtonShown);

			SetupPopupArrow(hint);

			TutorHintText.ForceMeshUpdate();
		}

		private void SetupPopupArrow(TutorHintEntity hint)
		{
			var isArrowShown = hint.IsPopupArrowShown;
			Arrow.Value()?.gameObject.SetActive(isArrowShown);

			if (!isArrowShown || Arrow.Value() == null)
				return;

			var offsetBetweenPopupAndHint = TutorHintBG.anchoredPosition - Unmask.rectTransform.anchoredPosition;

			var isHorizontalOffset = Mathf.Abs(offsetBetweenPopupAndHint.x) > Mathf.Abs(offsetBetweenPopupAndHint.y);

			var isPositiveOffset = (isHorizontalOffset
				? offsetBetweenPopupAndHint.x
				: offsetBetweenPopupAndHint.y) > 0;

			if (isHorizontalOffset)
			{
				Arrow.rectTransform.anchorMin = new Vector2(isPositiveOffset
						? 0
						: 1,
					0.5f);
				Arrow.rectTransform.anchorMax = new Vector2(isPositiveOffset
						? 0
						: 1,
					0.5f);
			}
			else
			{
				Arrow.rectTransform.anchorMin = new Vector2(0.5f,
					isPositiveOffset
						? 0
						: 1);
				Arrow.rectTransform.anchorMax = new Vector2(0.5f,
					isPositiveOffset
						? 0
						: 1);
			}

			Arrow.rectTransform.anchoredPosition = Vector2.zero;

			var arrowRotation = 90 * (isHorizontalOffset
				? 0
				: 1);

			arrowRotation += 90 * (isPositiveOffset
				? -1
				: 1);

			Arrow.transform.localRotation = Quaternion.Euler(0, 0, arrowRotation);
		}

		private void Resize()
		{
			for (var i = 0; i < hintTargets.Length; i++)
			{
				var unmask = unmaskPool[i];
				var hintTarget = hintTargets[i];

				var unmaskHint = tutorHintEntity.UnmaskHints.Count == 1
					? tutorHintEntity.UnmaskHints.First()
					: tutorHintEntity.UnmaskHints.ElementAt(i);

				unmask.rectTransform.sizeDelta = GetSizeDelta(unmaskHint, hintTarget);
			}
		}

		private void Reposition()
		{
			for (var i = 0; i < hintTargets.Length; i++)
			{
				var unmask = unmaskPool[i];
				var hintTarget = hintTargets[i];
				var unmaskHint = tutorHintEntity.UnmaskHints.Count == 1
					? tutorHintEntity.UnmaskHints.First()
					: tutorHintEntity.UnmaskHints.ElementAt(i);

				unmask.rectTransform.anchoredPosition = CalculateAnchoredPosition(unmaskHint, hintTarget);
			}
		}

		private Vector2 GetSizeDelta(UnmaskHint hint, HintTarget hintTarget)
		{
			if (hintTarget is not RectTransformHintTarget rectTransformHintTarget)
				return hint.HintVector.SizeDelta;

			var sizeDelta = hint.HintVector.IsAutoSize
				? rectTransformHintTarget.SizeDelta
				: hint.HintVector.SizeDelta;

			return sizeDelta;
		}

		private Vector2 CalculateAnchoredPosition(UnmaskHint hint, HintTarget hintTarget)
		{
			if (hintTarget is RectTransformHintTarget rectTransformHintTarget)
				return rectTransformHintTarget.AnchoredPosition + (Vector2)hint.HintVector.Position;

			var offset = hint.HintVector.Position;
			var viewportPosition = GetCurrentCamera().WorldToViewportPoint(hintTarget.transform.position);
			var worldObjectScreenPosition = new Vector2(
				(viewportPosition.x + offset.x) * targetCanvasRectTransform.sizeDelta.x -
				targetCanvasRectTransform.sizeDelta.x * 0.5f,
				(viewportPosition.y + offset.y) * targetCanvasRectTransform.sizeDelta.y -
				targetCanvasRectTransform.sizeDelta.y * 0.5f
			);

			return worldObjectScreenPosition;
		}

		protected virtual Camera GetCurrentCamera()
		{
			return targetCamera ? targetCamera : targetCamera = Camera.main;
		}
		protected override void OnClosed()
		{
			unmaskPool.ForEach(unmask => unmask.gameObject.SetActive(false));
			TutorHider.interactable = false;
			hintTargets = null;
			tutorHintEntity = null;
		}
	}
}