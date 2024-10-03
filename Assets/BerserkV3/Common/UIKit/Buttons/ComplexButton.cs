using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public enum ComplexButtonStyle
	{
		Solid = 0,
		Wire0 = 1,
		Wire1 = 2,
		Text = 3,
	}

	[Flags]
	public enum ComplexButtonColorAffect
	{
		All = -1,
		Text = 4,
		Icon = 8,
		Button = 16,
	}

	[Flags]
	public enum TriggerAction
	{
		All = -1,
		Enter = 4,
		Exit = 8,
		Click = 16,
		StyleChange = 32,
		IgnoreInteractable = 64,
	}

	[Serializable]
	public class ComplexButtonColor
	{
		public ComplexButtonColorAffect Affect = 0;
		public ComplexButtonStyle Style;
		public TriggerAction Action = 0;
		public Color Color = Color.white;
	}

	public class ComplexButton : BaseView
	{
		[SerializeField] private ComplexButtonStyle buttonStyle;
		[SerializeField] private bool enableIcon;
		[SerializeField] private bool enableMainText;
		[SerializeField] private bool interactable = true;
		[SerializeField] private bool flipIconText = true;

		[Header("Custom Fields")]
		[SerializeField] protected ExtendedButton SolidButton;
		[SerializeField] protected ExtendedButton Wire0Button;
		[SerializeField] protected ExtendedButton Wire1Button;
		[SerializeField] protected ExtendedButton TextButton;
		[SerializeField] protected TextMeshProUGUI ButtonText;
		[SerializeField] protected Image IconImage;
		[SerializeField] protected HorizontalOrVerticalLayoutGroup ContentLayout;
		[SerializeField] protected CanvasGroup ContentGroup;
		[SerializeField] protected List<ComplexButtonColor> Colors = new();

		private bool initialized;

		private void OnEnable()
		{
			if (initialized)
				return;

			initialized = true;
			SetStyle(buttonStyle);
			SetActiveText(enableMainText);
			SetActiveIcon(enableIcon);

			TriggerActionSubscribe(SolidButton);
			TriggerActionSubscribe(Wire0Button);
			TriggerActionSubscribe(Wire1Button);
			TriggerActionSubscribe(TextButton);
			ContentGroup.blocksRaycasts = false;
		}

		private void OnDestroy()
		{
			Clear();
			if (!initialized)
				return;
			
			SolidButton.UnscribeAllExtended();
			Wire0Button.UnscribeAllExtended();
			Wire1Button.UnscribeAllExtended();
			TextButton.UnscribeAllExtended();
			initialized = false;
		}

		private void TriggerActionSubscribe(ExtendedButton button)
		{
			button.onClick.AddListener(() => OnTriggerAction(TriggerAction.Click));
			button.onEnter.AddListener(() => OnTriggerAction(TriggerAction.Enter));
			button.onExit.AddListener(() => OnTriggerAction(TriggerAction.Exit));
		}

		public void SetStyle(ComplexButtonStyle value)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			buttonStyle = value;
			SetupText();
			SetActive(SolidButton, value == ComplexButtonStyle.Solid);
			SetActive(Wire0Button, value == ComplexButtonStyle.Wire0);
			SetActive(Wire1Button, value == ComplexButtonStyle.Wire1);
			SetActive(TextButton, value == ComplexButtonStyle.Text);
			OnTriggerAction(TriggerAction.StyleChange);
		}

		public void SetActiveText(bool value)
		{
			enableMainText = value;
			if (!SetupText())
				return;

			SetActive(ButtonText, value);
		}

		public void SetFlipTextIcon(bool value)
		{
			Assert(ContentLayout);
			ContentLayout.reverseArrangement = value;
		}

		public void SetText(string value)
		{
			if (!SetupText())
				return;

			Set(ButtonText, value);
		}

		public void SetColor(ComplexButtonColor color)
		{
			if (color.Style != buttonStyle)
				return;

			if ((color.Affect & ComplexButtonColorAffect.Text) > 0)
				SetTextColor(color.Color);

			if ((color.Affect & ComplexButtonColorAffect.Icon) > 0)
				SetIconColor(color.Color);

			if ((color.Affect & ComplexButtonColorAffect.Button) > 0)
				SetButtonColor(color.Color);
		}

		public void SetActiveIcon(bool value)
		{
			enableIcon = value;
			Assert(IconImage);
			SetActive(IconImage, value);
		}

		public void SetIcon(Sprite value)
		{
			Assert(IconImage);
			Set(IconImage, value);
		}

		public void SetTextColor(Color value)
		{
			if (!SetupText())
				return;

			Set(ButtonText, value);
		}

		public void SetIconColor(Color value)
		{
			Assert(IconImage);
			Set(IconImage, value);
		}

		public void SetButtonColor(Color value)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			Set(SolidButton.targetGraphic, value);
			Set(Wire0Button.targetGraphic, value);
			Set(Wire1Button.targetGraphic, value);
			if (TextButton.targetGraphic)
				Set(TextButton.targetGraphic, value);
		}

		public void Subscribe(Action action)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			SolidButton.onClick.AddListener(() => action?.Invoke());
			Wire0Button.onClick.AddListener(() => action?.Invoke());
			Wire1Button.onClick.AddListener(() => action?.Invoke());
			TextButton.onClick.AddListener(() => action?.Invoke());
		}

		public void Subscribe(Func<Task> action)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			SolidButton.onClick.AddListener(() => action?.Invoke());
			Wire0Button.onClick.AddListener(() => action?.Invoke());
			Wire1Button.onClick.AddListener(() => action?.Invoke());
			TextButton.onClick.AddListener(() => action?.Invoke());
		}

		public void Subscribe(Func<UniTask> action)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			SolidButton.onClick.AddListener(() => action?.Invoke());
			Wire0Button.onClick.AddListener(() => action?.Invoke());
			Wire1Button.onClick.AddListener(() => action?.Invoke());
			TextButton.onClick.AddListener(() => action?.Invoke());
		}

		public void SubscribeUnityEvent(UnityAction action)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			SolidButton.onClick.AddListener(action);
			Wire0Button.onClick.AddListener(action);
			Wire1Button.onClick.AddListener(action);
			TextButton.onClick.AddListener(action);
		}

		public void UnsubscribeUnityEvent(UnityAction action)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			SolidButton.onClick.RemoveListener(action);
			Wire0Button.onClick.RemoveListener(action);
			Wire1Button.onClick.RemoveListener(action);
			TextButton.onClick.RemoveListener(action);
		}

		public void UnsubscribeAll()
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			SolidButton.UnscribeAll();
			Wire0Button.UnscribeAll();
			Wire1Button.UnscribeAll();
			TextButton.UnscribeAll();
		}

		public void SetInteractable(bool interactable)
		{
			Assert(SolidButton, Wire0Button, Wire1Button, TextButton);
			this.interactable = interactable;
			CanvasGroup.alpha = interactable ? 1 : 0.3f;
			SolidButton.interactable = interactable;
			Wire0Button.interactable = interactable;
			Wire1Button.interactable = interactable;
			TextButton.interactable = interactable;
		}

		public void Clear()
		{
			UnsubscribeAll();
		}

		private void OnTriggerAction(TriggerAction action)
		{
			Colors.Where(c => (c.Action & action) != 0)
				.Where(c => interactable || (c.Action & TriggerAction.IgnoreInteractable) != 0)
				.ForEach(SetColor);
		}

		private bool SetupText()
		{
			if (!ButtonText)
				ButtonText = ContentLayout.GetComponentInChildren<TextMeshProUGUI>();

			if (!ButtonText)
			{
				TextButton.targetGraphic = IconImage;
				return false;
			}

			if (TextButton.targetGraphic != ButtonText)
				TextButton.targetGraphic = ButtonText;
			return true;
		}

		private static void Assert(params Component[] value)
		{
			if (!value.All(component => component.Value()))
				throw new NullReferenceException("Check components in inspector");
		}

		private void OnValidate()
		{
			SetStyle(buttonStyle);
			SetFlipTextIcon(flipIconText);
			SetActiveText(enableMainText);
			SetInteractable(interactable);
			SetActiveIcon(enableIcon);
		}
	}
}