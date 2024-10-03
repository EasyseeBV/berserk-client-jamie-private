using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RR.Core.DebugSystem;
using UnityEngine;

namespace RR.Core.Extensions
{
	public static class StringExtensions
	{
		public static string ToWhiteSpaceByUpperCase(this string str, string separator = " ")
		{
			var result = Regex.Split(str, "(?=\\p{Lu})");
			return result.JoinToString(separator);
		}
		public static string ToTitleCase(this string str)
			=> CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);

		public static DateTime ToDateTime(this string input) =>
			DateTime.TryParse(input, out var result)
				? result
				: new DateTime();

		public static DateTime ToUtcDateTime(this string input) =>
			DateTime.TryParse(input, CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result)
				? result
				: new DateTime();

		public static Color HexToColor(this string hex)
		{
			if (String.IsNullOrEmpty(hex))
			{
				RRLogger.Error("Hex string is null");
				return Color.white;
			}

			hex = hex.TrimStart('#');

			if (hex.Length < 6 || hex.Length > 8)
			{
				RRLogger.Error("Hex string has wrong format. Must be #FF00AA");
				return Color.white;
			}

			try
			{
				var r = Byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
				var g = Byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
				var b = Byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

				if (hex.Length == 8)
				{
					var a = Byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
					return new Color32(r, g, b, a);
				}

				return new Color32(r, g, b, 255);
			}
			catch (Exception e)
			{
				RRLogger.Error(e, $"Error parsing color. Used default {"Red".Red().Bold()} color.");
			}

			return Color.red;
		}

		public static string ColorToHex(this Color32 color)
		{
			return ColorUtility.ToHtmlStringRGB(color);
		}

		public static string ColorToHex(this Color color)
		{
			return ColorUtility.ToHtmlStringRGBA(color);
		}

		public static string ToUuid(this string input) => input.ToMd5Hash().Md5ToUuid();

		private static string Md5ToUuid(this string md5)
			=> $"{md5.Substring(0, 8)}-{md5.Substring(8, 4)}-{md5.Substring(12, 4)}-{md5.Substring(16, 4)}-{md5.Substring(20)}";

		public static string ToMd5Hash(this string input)
		{
			var hash = new StringBuilder();
			using (var md5Provider = new MD5CryptoServiceProvider())
			{
				var bytes = md5Provider.ComputeHash(new UTF8Encoding().GetBytes(input));

				foreach (var t in bytes)
					hash.Append(t.ToString("x2"));
			}

			return hash.ToString();
		}

		public static string ReplaceLastOccurrence(this string source, string find, string replace, bool ignoreCase = false)
		{
			var index = ignoreCase ?
				source.LastIndexOf(find, StringComparison.OrdinalIgnoreCase)
				: source.LastIndexOf(find, StringComparison.Ordinal);

			return index == -1
				? source
				: source
					.Remove(index, find.Length)
					.Insert(index, replace);
		}

		public static bool Same(this string value, string other)
			=> value?.Equals(other, StringComparison.OrdinalIgnoreCase) ?? other == null;

		public static string Ellipsis(this string value, int maxChars)
			=> value.Length <= maxChars
			? value
			: $"{value.Substring(0, maxChars)}...";

		public static void CopyToClipboard(this string value)
		{
			var te = new TextEditor { text = value };
			te.SelectAll();
			te.Copy();
		}

		/// <summary>
		/// converts a string to dash separated lowercase
		/// "lineBreaking" will become "line-breaking"
		/// </summary>
		private static string ToDashedCase(this string input)
			=> string.IsNullOrEmpty(input)
				? input
				: (input[0] + Regex.Replace(input.Substring(1), "([A-Z])", "-$0", RegexOptions.Compiled))
					  .ToLower();

		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source?.IndexOf(toCheck, comp) >= 0;
		}

		/// <summary>
		///     Converts a string to an extended selection matching string using an html tag.
		/// </summary>
		public static string ToHighlightMatchText(this string source, string textToCompare, string openedTag,
			string closedTag, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(textToCompare))
				return source;

			var firstMatchIndex = source.IndexOf(textToCompare, comparison);

			if (firstMatchIndex < 0)
				return string.Empty;

			var lastMatchIndex = firstMatchIndex + textToCompare.Length;

			var variantNameBuilder = new StringBuilder();
			if (firstMatchIndex > 0)
				variantNameBuilder.Append(source[..firstMatchIndex]);

			variantNameBuilder.Append(openedTag);
			variantNameBuilder.Append(source[firstMatchIndex..lastMatchIndex]);
			variantNameBuilder.Append(closedTag);

			if (lastMatchIndex < source.Length)
				variantNameBuilder.Append(source[lastMatchIndex..]);

			return variantNameBuilder.ToString();
		}

		/// <summary>
		///     Integers from -3999 to 3999 (from -MMMCMXCIX to MMMCMXCIX) are correctly converted.
		/// </summary>
		public static string ToRoman(this int number)
		{
			switch (number)
			{
				case 0:
					return "N";
				case < -3999 or > 3999:
					throw new ArgumentException("Value must be in the range -3999 - 3,999.");
			}

			//Further conversion requires non-standard characters
			int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
			string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
			var stringBuilder = new StringBuilder();

			var numberAbs = Math.Abs(number);
			for (var i = 0; i < numerals.Length; ++i)
				while (numberAbs >= values[i])
				{
					numberAbs -= values[i];
					stringBuilder.Append(numerals[i]);
				}

			if (number < 0)
				stringBuilder.Insert(0, "-");

			return stringBuilder.ToString();
		}


		/// <summary>
		/// //Unity optimization https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity5.html
		/// </summary>
		public static bool OptimizedEndsWith(this string a, string b)
		{
			var ap = (a?.Length ?? 0) - 1;
			var bp = (b?.Length ?? 0) - 1;

			while (ap >= 0 && bp >= 0 && a[ap] == b[bp])
			{
				ap--;
				bp--;
			}

			return bp < 0;
		}

		/// <summary>
		/// //Unity optimization https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity5.html
		/// </summary>
		public static bool OptimizedStartsWith(this string a, string b)
		{
			var aLen = a?.Length ?? 0;
			var bLen = b?.Length ?? 0;

			var ap = 0;
			var bp = 0;

			while (ap < aLen && bp < bLen && a[ap] == b[bp])
			{
				ap++;
				bp++;
			}

			return bp == bLen;
		}

	}
}