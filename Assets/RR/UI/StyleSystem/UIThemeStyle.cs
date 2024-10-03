using RR.Core.DebugSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RR.UI.StyleSystem
{
	/// <summary>
	/// STYLESHEET FORMAT RULE
	/// 0 - Background (*bg)
	/// 1 - Title (titleBg)
	/// 2 - Controls (*btn)
	/// 3 - Border (*border-)
	/// 4 - Text (*txt, *text)
	///
	/// If palette has less elements, no change will be made to corresponding elements.
	/// </summary>
	public class UIThemeStyle : MonoBehaviour
	{
		//todo: make a more generic stylizer with use of sprite change option
		[InfoBox("STYLESHEET FORMAT RULE" +
				 "\r\n\r\n\t 0 - Background (*bg)" +
				 "\r\n\t 1 - Title (*title)" +
				 "\r\n\t 2 - Controls (*btn, *slider)" +
				 "\r\n\t 3 - Text (*txt, *text)" +
				 "\r\n\t 4 - Border (*border)" +
				 "\r\n\r\n If palette has less elements, no change will be made to corresponding elements.")]

		[SerializeField, LabelText("Selected Style Content")]
		public List<Color> StyleSheet;

		public void ApplyStyles(List<Color> styles = null)
		{
			if (StyleSheet == null && styles == null)
			{
				RRLogger.Warning($"There is no {nameof(StyleSheet)} or {nameof(styles)} to load.");
				return;
			}

			if (styles != null)
				StyleSheet = styles;

			foreach (var graphics in GetComponentsInChildren<Graphic>(true))
			{
				switch (graphics.name.ToLower())
				{
					case var a when a.Contains("slider"):
					case var b when b.Contains("btn"): if (StyleSheet.Count > 2) graphics.color = StyleSheet[2]; break;
					case var a when a.Contains("txt"):
					case var b when b.Contains("text"): if (StyleSheet.Count > 3) graphics.color = StyleSheet[3]; break;
					case var a when a.Contains("title"): if (StyleSheet.Count > 1) graphics.color = StyleSheet[1]; break;
					case var a when a.Contains("bg"): if (StyleSheet.Count > 0) graphics.color = StyleSheet[0]; break;
					case var a when a.Contains("border"): if (StyleSheet.Count > 4) graphics.color = StyleSheet[4]; break;
				}
			}
		}

#if UNITY_EDITOR

		[ShowIf(nameof(isStylesNotEmpty)),
		 PropertyOrder(-1),
		 OnValueChanged(nameof(ExtractStyles)),
		 ValueDropdown(nameof(stylePresets))]
		public Object selectedStyle;

		[HideInInspector, NonSerialized]
		public List<Object> stylePresets;
		private bool isStylesNotEmpty => stylePresets != null;

		[Button("Apply")]
		private void ApplyStyles() => ApplyStyles(null);

		// According to https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/PresetLibraries/ColorPresetLibrary.cs
		[PropertySpace, Button("Refresh")]
		private void ExtractStyles()
		{
			if (selectedStyle == null)
			{
				StyleSheet = null;
				return;
			}

			var presets = ((IEnumerable<object>)selectedStyle
				.GetType()
				.GetField("m_Presets", BindingFlags.NonPublic | BindingFlags.Instance)?
				.GetValue(selectedStyle));

			if (presets == null)
			{
				RRLogger.Error($"{nameof(selectedStyle)} has no [m_Presets] variable.");
				return;
			}

			var colors = presets.Select(x =>
			{
				var colorField = x.GetType().GetField("m_Color", BindingFlags.NonPublic | BindingFlags.Instance);
				if (colorField == null)
				{
					RRLogger.Error($"{x.GetType().Name} has no [m_Color] variable");
					return Color.red;
				}

				return (Color)colorField.GetValue(x);
			});

			StyleSheet = colors.ToList();
		}

		[Button("Extract Current View Style")]
		private void ExtractCurrentStyle()
		{
			selectedStyle = null;
			StyleSheet = new List<Color>();

			var graphics = GetComponentsInChildren<Graphic>(true);

			StyleSheet.Add(graphics.FirstOrDefault(x => x.name.ToLower().Contains("bg"))?.color ?? Color.white);
			StyleSheet.Add(graphics.FirstOrDefault(x => x.name.ToLower().Contains("title"))?.color ?? Color.white);

			StyleSheet.Add(graphics.FirstOrDefault(x => x.name.ToLower().Contains("slider")
														  || x.name.ToLower().Contains("btn"))?.color ?? Color.white);

			StyleSheet.Add(graphics.FirstOrDefault(x => x.name.ToLower().Contains("txt")
														  || x.name.ToLower().Contains("text"))?.color ?? Color.white);

			StyleSheet.Add(graphics.FirstOrDefault(x => x.name.ToLower().Contains("border"))?.color ?? Color.white);

			// todo: add btn to transform result into a color palette
		}
#endif
	}
}
