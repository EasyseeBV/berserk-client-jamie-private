using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RR.UI.Custom
{
	[RequireComponent(typeof(Button))]
	public class RRButton : RRUIBehavior, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
	{
		public UnityEvent OnPress;
		public UnityEvent OnHold;
		public UnityEvent OnRelease;
		public UnityEvent OnHover;

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private RawImage rawImage;
		[SerializeField] private Image image;

		[SerializeField, HideInInspector] private Button button;

		private bool isHolding;

		public bool IsInteractable => button.interactable;
		public string Text => label.text;
		public Color Color => button.targetGraphic?.color ?? rawImage?.color ?? image?.color ?? button.colors.normalColor;

		#region UnityCallbacks

		private void OnDestroy()
		{
			button.onClick.RemoveAllListeners();
		}

		private void Update()
		{
			if (isHolding)
				OnHold.Invoke();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isHolding = true;
			OnPress.Invoke();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			isHolding = false;
			OnRelease.Invoke();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			OnHover.Invoke();
		}

		#endregion

		/// <summary>
		/// Implicitly casts .ToSting() on the object.
		/// Checks for null.
		/// </summary>
		public RRButton SetText(object value) => SetText(value?.ToString());
		public RRButton SetText(string value)
		{
			if (label)
				label.SetText(value);
			else
				Debug.LogError($"{nameof(SetText)} on {nameof(gameObject.name)} cant be executed, {nameof(label)} is null. {nameof(value)}=[{value}]");

			return this;
		}

		public RRButton SetGraphics(Texture2D texture)
		{
			if (rawImage) rawImage.texture = texture;
			else if (image) image.sprite = Sprite.Create(texture, Rect.zero, Vector2.one / 2, 100);
			else
				Debug.LogError($"{nameof(SetGraphics)} on {nameof(gameObject.name)} cant be executed, graphics is null.");
			return this;
		}

		public RRButton SetGraphics(Sprite sprite)
		{
			if (rawImage) rawImage.texture = sprite.texture;
			else if (image) image.sprite = sprite;
			else
				Debug.LogError($"{nameof(SetGraphics)} on {nameof(gameObject.name)} cant be executed, graphics is null.");
			return this;
		}

		public RRButton SetGraphicsColor(Color color)
		{
			if (button.targetGraphic)
				button.targetGraphic.color = color;

			if (rawImage)
				rawImage.color = color;
			else if (image)
				image.color = color;

			return this;
		}

		public RRButton SetTextColor(Color color)
		{
			label.color = color;

			return this;
		}

		/// <summary>
		/// Same as <see cref="OnClick"/>
		/// </summary>
		public void Subscribe(Action action) => OnClick(action);
		public void OnClick(Action action)
		{
			button.onClick.AddListener(action.Invoke);
		}

		public void RemoveListener(UnityAction unityAction)
		{
			button.onClick.RemoveListener(unityAction);
		}

		public void RemoveAllListeners()
		{
			button.onClick.RemoveAllListeners();
		}

		public void SetInteractable(bool isInteractable = true)
		{
			button.interactable = isInteractable;
		}

		public static implicit operator Button(RRButton btn) => btn.button;

#if UNITY_EDITOR

		protected void Reset()
		{
			button = gameObject.GetComponent<Button>();

			rawImage = GetComponentInChildren<RawImage>(true);
			image = GetComponentInChildren<Image>(true);
			label = GetComponentInChildren<TextMeshProUGUI>(true);
		}

#endif
	}
}