using System.Globalization;
using TMPro;

namespace RR.UI.Extensions
{
	public static class TMPExtensions
	{
#if !NO_TEXTMESHPRO

		public static void SetText(this TextMeshProUGUI tmpText, decimal value, string format = "")
					=> tmpText.SetText(value.ToString(format, CultureInfo.InvariantCulture));	
		public static void SetText(this TextMeshProUGUI tmpText, int value, string format = "")
					=> tmpText.SetText(value.ToString(format, CultureInfo.InvariantCulture));
		public static void SetText(this TextMeshProUGUI tmpText, float value, string format = "")
					=> tmpText.SetText(value.ToString(format, CultureInfo.InvariantCulture));
		public static void SetText(this TextMeshProUGUI tmpText, object value)
					=> tmpText.SetText(value.ToString());

#endif
	}
}
