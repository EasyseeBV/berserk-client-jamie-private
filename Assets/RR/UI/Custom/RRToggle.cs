using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RR.UI.Custom
{
	[RequireComponent(typeof(Toggle))]
	public class RRToggle : RRUIBehavior, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDisposable
	{
		public event Action OnPress;
		public event Action OnHold;
		public event Action OnRelease;
		public event Action OnSelect;
		public event Action OnDeselect;
		public event Action OnHover;
		public event Action OnWithdrawal;

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private RawImage rawImage;
		[SerializeField] private Image image;
		[SerializeField] private RawImage rawImageCheckmark;
		[SerializeField] private Image imageCheckmark;
		[SerializeField] private Toggle toggle;

		private bool isHolding;

		public string Text => label.text;

		public Color ColorImage => toggle.targetGraphic?.color ?? rawImage?.color ?? image?.color ?? toggle.colors.normalColor;

		public Color ColorCheckmark => toggle.graphic?.color ?? rawImageCheckmark?.color ?? imageCheckmark?.color ?? toggle.colors.normalColor;

		public bool IsOn { get; private set; }

		#region UnityCallbacks

		private void OnEnable()
		{
			toggle.onValueChanged.AddListener(SelectDeselectCall);
			IsOn = toggle.isOn;
		}

		private void OnDisable()
		{
			toggle.onValueChanged.RemoveListener(SelectDeselectCall);
		}

		private void OnDestroy()
		{
			Dispose();
		}

		private void Update()
		{
			if (isHolding)
				OnHold?.Invoke();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			OnHover?.Invoke();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isHolding = true;
			OnPress?.Invoke();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			isHolding = false;
			OnRelease?.Invoke();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			OnWithdrawal?.Invoke();
		}

		private void SelectDeselectCall(bool value)
		{
			if (IsOn == value)
				return;
			if (value)
				OnSelect?.Invoke();
			else
				OnDeselect?.Invoke();
			IsOn = value;
		}

		#endregion

		public RRToggle SetTextColor(Color color)
		{
			label.color = color;
			return this;
		}

		public RRToggle Select()
		{
			return ValueChange();
		}

		public RRToggle Deselect()
		{
			return ValueChange(false);
		}

		public RRToggle ValueChange(bool flag = true)
		{
			toggle.isOn = flag;
			SelectDeselectCall(flag);
			return this;
		}

		/// <summary>
		///     Implicitly casts .ToSting() on the object.
		///     Checks for null.
		/// </summary>
		public RRToggle SetText(object value)
		{
			return SetText(value?.ToString());
		}

		public RRToggle SetText(string value)
		{
			if (label)
				label.SetText(value);
			else
				Debug.LogError(
					$"{nameof(SetText)} on {nameof(gameObject.name)} cant be executed, {nameof(label)} is null. {nameof(value)}=[{value}]");

			return this;
		}

		public RRToggle SetImageGraphics(Texture2D texture)
		{
			return SetGraphics(rawImage, image, texture);
		}

		public RRToggle SetCheckmarkGraphics(Texture2D texture)
		{
			return SetGraphics(rawImageCheckmark, imageCheckmark, texture);
		}

		public RRToggle SetImageGraphics(Sprite sprite)
		{
			return SetGraphics(rawImage, image, sprite);
		}

		public RRToggle SetCheckmarkGraphics(Sprite sprite)
		{
			return SetGraphics(rawImageCheckmark, imageCheckmark, sprite);
		}

		public RRToggle SetImageGraphicsColor(Color color)
		{
			return SetGraphicsColor(rawImage, image, color);
		}

		public RRToggle SetCheckmarkGraphicsColor(Color color)
		{
			return SetGraphicsColor(rawImageCheckmark, imageCheckmark, color);
		}

		private RRToggle SetGraphics(RawImage rImg, Image img, Texture2D texture)
		{
			if (rImg) rImg.texture = texture;
			else if (img) img.sprite = Sprite.Create(texture, Rect.zero, Vector2.one / 2, 100);
			else
				Debug.LogError(
					$"{nameof(SetGraphics)} on {nameof(gameObject.name)} cant be executed, graphics is null.");
			return this;
		}

		private RRToggle SetGraphics(RawImage rImg, Image img, Sprite sprite)
		{
			if (rImg) rImg.texture = sprite.texture;
			else if (img) img.sprite = sprite;
			else
				Debug.LogError(
					$"{nameof(SetGraphics)} on {nameof(gameObject.name)} cant be executed, graphics is null.");
			return this;
		}

		private RRToggle SetGraphicsColor(RawImage rImg, Image img, Color color)
		{
			if (toggle.targetGraphic)
				toggle.targetGraphic.color = color;

			if (rImg)
				rImg.color = color;
			else if (img)
				img.color = color;

			return this;
		}

		/// <summary>
		///     Same as <see cref="OnValueChanged" />
		/// </summary>
		public void Subscribe(Action<bool> action)
		{
			OnValueChanged(action);
		}

		public void OnValueChanged(Action<bool> action)
		{
			toggle.onValueChanged.AddListener(action.Invoke);
		}

		public void RemoveListener(UnityAction<bool> unityAction)
		{
			toggle.onValueChanged.RemoveListener(unityAction);
		}

		public void RemoveAllListeners()
		{
			toggle.onValueChanged.RemoveAllListeners();
		}

		public void SetInteractable(bool isInteractable = true)
		{
			toggle.interactable = isInteractable;
		}

		public static implicit operator Toggle(RRToggle tgl)
		{
			return tgl.toggle;
		}

#if UNITY_EDITOR

		protected void Reset()
		{
			toggle = gameObject.GetComponent<Toggle>();
			label = GetComponentInChildren<TextMeshProUGUI>(true);

			image = (Image)toggle.targetGraphic;
			imageCheckmark = (Image)toggle.graphic;

			var rImgs = GetComponentsInChildren<RawImage>(true);
			var imgs = GetComponentsInChildren<Image>(true);

			rawImage = rImgs.Length > 0 ? rImgs[0] : null;
			if (image == null)
				image = imgs.Length > 0 ? imgs[0] : null;

			rawImageCheckmark = rImgs.Length > 1 ? rImgs[1] : null;
			if (imageCheckmark == null)
				imageCheckmark = imgs.Length > 1 ? imgs[1] : null;
		}

#endif
		public void Dispose()
		{
			toggle.onValueChanged.RemoveAllListeners();
			OnPress = null;
			OnHold = null;
			OnRelease = null;
			OnSelect = null;
			OnDeselect = null;
			OnHover = null;
			OnWithdrawal = null;
		}
	}
}