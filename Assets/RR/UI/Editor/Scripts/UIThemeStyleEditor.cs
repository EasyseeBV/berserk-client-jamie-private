using RR.Core.Editor;
using RR.UI.StyleSystem;
using Sirenix.OdinInspector.Editor;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RR.UI.Editor
{
	[CustomEditor(typeof(UIThemeStyle), true)]
	public class UIThemeStyleEditor : OdinEditor
	{
		private UIThemeStyle self;

		protected override void OnEnable()
		{
			base.OnEnable();
			self = target as UIThemeStyle;

			if (!self)
				return;

			RefreshPalettes();
		}

		private void RefreshPalettes()
		{
			AssetDatabase.Refresh();
			self.stylePresets = AssetUtility.FindAssetsWithExtension<Object>("colors").ToList();
			self.stylePresets.Add(default); // fit with no style option

			if (self.selectedStyle != null)
			{
				var n = self.selectedStyle.name;
				self.Invoke("ExtractStyles", 0);
				self.selectedStyle = self.stylePresets.FirstOrDefault(x => x.name == n);
			}
		}
	}
}