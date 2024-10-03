using System;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public class ComplexInputField : BaseView
	{
		[Header("Custom Fields")]
		[SerializeField] protected bool secureEnabled = true;
		[SerializeField] protected TMP_InputField InputField;
		[SerializeField] protected Toggle HideToggle;
		private TMP_InputField.ContentType? defaultType;
		
		private void OnEnable()
		{
			defaultType ??= InputField.contentType;
			HideToggle.onValueChanged.AddListener(SetProtectField);
			SetActiveSecurityField(secureEnabled);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Clear();
		}
		
		public string GetValue()
		{
			return InputField.text ?? string.Empty;
		}

		public void SetText(string value)
		{
			InputField.text = value;
		}

		public void SetToggle(bool isOn)
		{
			if (!HideToggle)
				return;
			
			HideToggle.SetIsOnWithoutNotify(isOn);
		}

		public void SetActiveToggle(bool value)
		{
			if (!HideToggle)
				return;

			HideToggle.gameObject.SetActive(value);
		}

		public void SetActiveSecurityField(bool value)
		{
			secureEnabled = value;
			SetActiveToggle(value);
			SetProtectField(value);
			SetToggle(value);
		}

		public void SetProtectField(bool isOn)
		{
			if (!Application.isPlaying || !InputField || !defaultType.HasValue || !secureEnabled)
				return;

			InputField.contentType = isOn ? TMP_InputField.ContentType.Password : defaultType.Value;
			InputField.ForceLabelUpdate();
		}

		public void SetValueChangeAction(Action<string> action)
		{
			InputField.onValueChanged.AddListener(value => action?.Invoke(value));
		}

		public void Clear()
		{
			SetProtectField(true);
			HideToggle.onValueChanged.RemoveAllListeners();
			InputField.onValueChanged.RemoveAllListeners();
			InputField.text = string.Empty;
		}

		private void OnValidate()
		{
			SetActiveSecurityField(secureEnabled);
		}
	}
}