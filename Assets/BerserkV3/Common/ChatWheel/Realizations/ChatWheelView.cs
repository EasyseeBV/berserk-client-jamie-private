using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BerserkV3.Generic.ChatWheel
{
	public partial class ChatWheelView : BaseView, IPointerExitHandler, IPointerEnterHandler, IChatWheel
	{
		[Header("Animation settings")]
		[SerializeField, Range(0.5f, 2000f)] private float radius = 5f;
		[SerializeField, Range(0f, 20f)] private float durationOut = 0.3f;
		[SerializeField, Range(0f, 20f)] private float durationIn = 0.15f;
		[SerializeField, Range(-1f, 1f)] private float startOffset = 0.25f;
		[SerializeField] private Ease ease = Ease.OutSine;

		private ChatWhellSlotView[] slots;
		private Sequence inOutTween;

		public event Action<bool> OnFocus;

		public RectTransform Container => RectTransform;

		public int SlotCount => slots?.Length ?? 0;

		public event Action<IChatWheelSlot> OnSlotClick;

		public event Action<IChatWheelSlot> OnSlotHovered;
		
		public event Action<IChatWheelSlot, IChatWheelElement> OnDroppedIn;

		protected override void OnAwake()
		{
			ChatWheelSlot.gameObject.SetActive(false);
		}

		public void Init(int slotCount)
		{
			if (slots != null)
				return;

			slots = new ChatWhellSlotView[slotCount];
			for (var i = 0; i < slotCount; i++)
			{
				var slot = Instantiate(ChatWheelSlot, Container, true);
				slot.Container.gameObject.SetActive(true);
				slot.SetVisibleState(VisibleState.Visible);
				slot.OnClick += sender => OnSlotClick?.Invoke(sender);
				slot.OnHovered += sender => OnSlotHovered?.Invoke(sender);
				slot.OnDroppedIn += (sender, element) => OnDroppedIn?.Invoke(sender, element);
				slot.Container.localPosition = Vector3.zero;
				slot.Container.localScale = Vector3.zero;
				slot.Init($"slot_{i}");
				slots[i] = slot;
			}
		}

		public void Show(bool force = false, Action onShowed = null)
		{
			if (VisibleState == VisibleState.Visible)
				return;

			base.Show(noAnimation: true);
			Animate(force ? 0 : durationOut, 1, onShowed);
		}

		public void Close(bool force = false, Action onClosed = null)
		{
			if (VisibleState != VisibleState.Visible)
				return;

			Animate(force ? 0 : durationIn, 0, () =>
			{
				Close(noAnimation: true);
				onClosed?.Invoke();
			});
		}

		public void Clear()
		{

			slots?.ForEach(x=> Destroy(x.Container.gameObject));
			slots = null;
			inOutTween?.Kill();
			inOutTween = null;
			OnSlotClick = null;
			OnSlotHovered = null;
			OnDroppedIn = null;
			OnFocus = null;
		}

		public void SwapSlotsElement(string fromSlotId, string toSlotId)
		{
			var from = ReplaceSlotElement(fromSlotId, null);
			var to = ReplaceSlotElement(toSlotId, from);
			
			if (fromSlotId != toSlotId)
				ReplaceSlotElement(fromSlotId, to);
		}

		/// <summary>
		/// Replace target slot entity a new entity and return old entity
		/// </summary>
		/// <param name="slotId">target slot id</param>
		/// <param name="element">new entity to set in slot</param>
		/// <returns>old entity from target slot</returns>
		public IChatWheelElement ReplaceSlotElement(string slotId, IChatWheelElement element)
		{
			var slot = slots.FirstOrDefault(x => x.Id == slotId);
			if (slot == null)
				throw new ArgumentException($"Target slot with id : {slotId} does not exist.");
			
			element?.OccupiedSlot?.Set(null);
			return slot.Set(element);

		}

		/// <summary>
		/// Set new entities with replace old entities and return them.
		/// </summary>
		/// <param name="elements">new entities</param>
		/// <returns>old entities</returns>
		public IEnumerable<IChatWheelElement> Set(params IChatWheelElement[] elements)
		{
			// do not use Linq because returned items need be iterate the
			// linq or yield iterator to work this method and Slot.Set() call
			if (elements == null || elements.Length == 0)
				return Array.Empty<IChatWheelElement>();
			
			var oldEntities = GetSlotElements();
			slots.ForEach((slot, i) => slot.Set(i < elements.Length ? elements[i] : null));
			return oldEntities;
		}

		/// <summary>
		/// Add new entities.
		/// </summary>
		/// <param name="elements">new entities</param>
		public void Add(params IChatWheelElement[] elements)
		{
			if (elements == null || elements.Length == 0)
				return;
			
			slots.Where(slot => slot.Element == null)
				.ForEach((slot, i) => slot.Set(i < elements.Length ? elements[i] : null));
		}

		/// <summary>
		/// Get existing entities in slots
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IChatWheelElement> GetSlotElements()
		{
			if (slots == null || slots.Length == 0)
				return Array.Empty<IChatWheelElement>();
			
			return slots
				.Select(x => x.Element)
				.Where(x => x != null)
				.ToArray();
		}

		/// <summary>
		/// Get existing slots
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IChatWheelSlot> GetSlots()
		{
			if (slots == null || slots.Length == 0)
				return Array.Empty<IChatWheelSlot>();
			
			return slots;
		}

		/// <summary>
		/// Get occupied slots
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IChatWheelSlot> GetOccupiedSlots()
		{
			if (slots == null || slots.Length == 0)
				return Array.Empty<IChatWheelSlot>();
			
			return slots
				.Where(x => x.Element != null)
				.ToArray();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			OnFocus?.Invoke(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			OnFocus?.Invoke(true);
		}

		private void Animate(float duration, float inOutScale, Action onComplete = null)
		{
			inOutTween?.Kill();
			inOutTween = DOTween.Sequence();
			var center = Vector3.zero;

			for (var i = 0; i < SlotCount; i++)
			{
				if (slots == null || i >= slots.Length)
					break;
				
				var slot = slots[i];
				var target = inOutScale > 0 ? GetCircleTarget(i) : center;
				slot.Interactable = false;
				inOutTween
					.Insert(0, slot.Container
								.DOLocalMove(target, duration)
								.OnComplete(() => slot.Interactable = true))
					.Insert(0, slot.Container.DOScale(inOutScale, duration));
			}

			inOutTween
				.SetEase(ease)
				.SetAutoKill(true)
				.OnComplete(() => onComplete?.Invoke())
				.Play();
		}

		private Vector3 GetCircleTarget(int iterator)
		{
			var currProgress = 1f - (((float) iterator / SlotCount) + startOffset);
			var currRadian = (currProgress * (2 * Mathf.PI));
			var x = Mathf.Cos(currRadian) * radius;
			var y = Mathf.Sin(currRadian) * radius;
			return new Vector3(x, y, 0);
		}
	}
}