using System;
using DG.Tweening;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BerserkV3.Generic.ChatWheel
{
	public class ChatWhellSlotView : BaseView,
									 IChatWheelSlot,
									 IPointerDownHandler,
									 IPointerEnterHandler,
									 IPointerExitHandler
	{
		[SerializeField] private float maxHoveredSize = 1.1f;
		[SerializeField] private float hoveringDuration = 0.5f;

		private readonly int hoverSubling = 20;
		private Tween hoverTween;
		private int initSubling;

		public RectTransform Container => RectTransform;

		public IChatWheelElement Element { get; private set; }

		public string Id { get; private set; }

		public bool Interactable { get; set; } = true;

		public event Action<IChatWheelSlot> OnClick;

		public event Action<IChatWheelSlot> OnHovered;
		
		public event Action<IChatWheelSlot, IChatWheelElement> OnDroppedIn;

		public void Init(string slotId)
		{
			Id = slotId;
			name = slotId;
			initSubling = RectTransform.GetSiblingIndex();
		}

		public IChatWheelElement Set(IChatWheelElement element)
		{
			if (element?.OccupiedSlot != null)
				throw new InvalidOperationException($"Cant set element with id : {element.Id} " +
				                                    $"because element already have " +
				                                    $"another slot with id : {element.OccupiedSlot.Id}");
			var old = Element;
			old?.Release();
			Element = element;
			Element?.Hold(this);

			return old;
		}

		public override bool CanDropIn(BaseView draggedView)
		{
			return draggedView is IChatWheelElement;
		}

		public override void DropIn(BaseView draggedView)
		{
			if (draggedView is not IChatWheelElement chatWheelElement)
				return;

			OnDroppedIn?.Invoke(this, chatWheelElement);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!Interactable)
				return;

			hoverTween?.Kill();
			hoverTween = Container.DOScale(Vector3.one * maxHoveredSize, hoveringDuration);
			RectTransform.SetSiblingIndex(hoverSubling);
			OnHovered?.Invoke(this);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!Interactable)
				return;

			hoverTween?.Kill();
			hoverTween = Container.DOScale(Vector3.one, hoveringDuration);
			RectTransform.SetSiblingIndex(initSubling);
			OnHovered?.Invoke(null);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (Element == null || !Interactable)
				return;

			OnClick?.Invoke(this);
		}

		private void OnDestroy()
		{
			Element = null;
			OnClick = null;
			OnHovered = null;
		}
	}
}