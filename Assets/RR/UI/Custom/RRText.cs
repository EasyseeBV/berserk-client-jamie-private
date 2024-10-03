using RR.Core.Extensions;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace RR.UI.Custom
{
	[RequireComponent(typeof(TMP_Text))]
	public class RRText : RRUIBehavior
	{
		[SerializeField, HideInInspector] private TMP_Text textMeshPro = default;

		public void SetColor(Color color)
		{
			textMeshPro.color = color;
		}

		public void SetText(string value)
		{
			textMeshPro.SetText(value);
		}

		public void SetText(TimeSpan value)
		{
			textMeshPro.SetText(value.ToShortString());
		}

		public void SetText(double value, DoubleFormat format = DoubleFormat.None)
		{
			switch (format)
			{
				case DoubleFormat.NoDot:
					textMeshPro.SetText($"{value:#}");
					break;

				case DoubleFormat.TwoSigned:
					textMeshPro.SetText($"{value:##.00}");
					break;

				case DoubleFormat.OneSigned:
					textMeshPro.SetText($"{value:##.0}");
					break;

				default:
					textMeshPro.SetText(value.ToString(CultureInfo.InvariantCulture));
					break;
			}
		}

		public static implicit operator TMP_Text(RRText txt) => txt.textMeshPro;

#if UNITY_EDITOR

		private void Reset()
		{
			textMeshPro = this.GetOrAddComponent<TextMeshProUGUI>();
		}
#endif

		public enum DoubleFormat
		{
			None = 0,
			NoDot = 1,
			TwoSigned = 2,
			OneSigned = 3
		}
	}
}
