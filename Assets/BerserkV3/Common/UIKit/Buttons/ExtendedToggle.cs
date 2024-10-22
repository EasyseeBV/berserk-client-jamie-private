using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	/// <inheritdoc />
	/// <summary>
	/// Use only with <see cref="T:BerserkV3.Common.UIKit.ExtendedToggleGroup" />, it doesn't work without group.
	/// </summary>
	public class ExtendedToggle : Toggle
	{
		[SerializeField] private TMP_Text textLabel;
		[SerializeField] private GameObject selector;
		
		public void SetText(string text)
		{
			textLabel.text = text;
		}
		
		public void SetSelectorActive(bool value)
		{
			if (selector)
				selector.SetActive(value);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			onValueChanged?.RemoveAllListeners();
		}
	}
}