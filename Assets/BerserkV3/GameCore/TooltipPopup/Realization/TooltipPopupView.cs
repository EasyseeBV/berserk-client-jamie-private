using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BerserkV3.GameCore.Cards.EffectHints;
using BerserkV3.GameCore.TooltipPopup.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.TooltipPopup
{
	public class TooltipPopupView : MonoBehaviour, ITooltipPopupView
	{
		[SerializeField] private Vector3 offset;
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private TooltipLayoutGroup layoutGroup;
		[SerializeField] private TooltipEffectView effectPrefab;
		
		private Transform targetAnchor;
		private RectTransform canvasRectTransform;

		private bool initialized;
		private EffectOrigin origin;
		
		public async UniTask InitAsync(List<TooltipEffectData> data, Transform parent, EffectOrigin origin, CancellationToken ctn = default)
		{
			if (!canvasRectTransform)
				canvasRectTransform = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
			
			this.origin = origin;
			targetAnchor = parent;
			SetPosition(parent.position);
			initialized = true;
			
			await FillAsync(data, ctn);
		}

		public void Show()
		{
			gameObject.SetActive(true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		public void Close()
		{
			gameObject.SetActive(false);
			initialized = false;
		}

		private UniTask FillAsync(IReadOnlyList<TooltipEffectData> data, CancellationToken ctn)
		{
			var existingViews = rectTransform.GetComponentsInChildren<TooltipEffectView>(true);

			var tasks = Enumerable.Range(0, Mathf.Max(existingViews.Length, data.Count)).Select(index =>
			{
				if (index >= data.Count)
				{
					if (index < existingViews.Length)
						existingViews[index].SetActive(false);

					return UniTask.CompletedTask;
				}

				if (index >= existingViews.Length)
					return Instantiate(effectPrefab, rectTransform).InitAsync(data[index], ctn);

				var view = existingViews[index];
				view.SetActive(true);
				return view.InitAsync(data[index], ctn);
			});
			
			return UniTask.WhenAll(tasks);
		}

		private void LateUpdate()
		{
			if (initialized && targetAnchor)
				SetPosition(targetAnchor.position);
		}

		private void SetPosition(Vector3 position)
		{
			var isInnate = origin == EffectOrigin.Innate;
			var pivot = new Vector2
			{
				x = !isInnate ? 1 : 0,
				y = 1f
			};
			
			rectTransform.pivot = pivot;
			rectTransform.localScale = Vector3.one;
			rectTransform.localEulerAngles = Vector3.zero;
			
			var screenHeight = canvasRectTransform.rect.size.y;
			var parentPos = canvasRectTransform.InverseTransformPoint(position) +
			                (!isInnate
				                ? new Vector3(-offset.x, offset.y)
				                : new Vector3(offset.x, offset.y));

			rectTransform.anchoredPosition = parentPos;
			
			if (parentPos.y > (screenHeight / 2) - layoutGroup.Margins.top)
				parentPos = new Vector3(parentPos.x, screenHeight / 2 - layoutGroup.Margins.top);

			var parentVerticalPosFromScreenBottom = parentPos.y + screenHeight / 2 - layoutGroup.Margins.bottom;

			if (parentVerticalPosFromScreenBottom < rectTransform.rect.size[1])
			{
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 
					parentPos.y + (rectTransform.rect.size[1] - parentVerticalPosFromScreenBottom));
			}
		}
	}
}
