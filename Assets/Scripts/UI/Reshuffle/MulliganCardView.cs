using DG.Tweening;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vulcan.Data;

namespace UI
{
	public partial class MulliganCardView : BaseView, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private Button selectButton = default;

		private static readonly float CARD_SCALE = 1.5f;
		private static readonly float CARD_HOVER_SCALE = 1.7f;

		public CardData Data => data;
		public bool Selected => selected;

		private CardData data;
		private bool selected = false;
		private bool locked = false;

		protected override void OnAwake()
		{
			selectButton.onClick.AddListener(OnSelect);
		}

		public MulliganCardView SetUp(CardData data)
		{
			this.data = data;
			HandCardView.UpdateValues(data);
			HandCardView.CanvasGroup.interactable = true;
			HandCardView.enabled = false;

			selected = false;
			ExchangeTxt.gameObject.SetActive(false);
			transform.localScale = CARD_SCALE * Vector3.one;

			ShowSelection();
			return this;
		}

		public void Lock()
		{
			locked = true;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;
			HandCardView.SetShine(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			transform.DOKill();
			transform.DOScale(CARD_HOVER_SCALE, 0.2f).SetEase(Ease.InOutCubic);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.DOKill();
			transform.DOScale(CARD_SCALE, 0.2f).SetEase(Ease.InOutCubic);
		}

		private void OnSelect()
		{
			if (locked)
				return;

			selected = !selected;
			ShowSelection();
		}

		private void ShowSelection()
		{
			HandCardView.SetShine(!selected);
			ExchangeTxt.gameObject.SetActive(selected);
			CanvasGroup.alpha = selected ? 0.5f : 1f;
		}
	}
}