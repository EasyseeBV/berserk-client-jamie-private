using System;
using System.Linq;

namespace Berserk.Shared.GameCore.Utils
{
	public static class SharedStringExtensions
	{
		/// <summary>
		/// Will format any public fields and their values into a string.
		/// </summary>
		public static string ReflectionFormat(this object source)
		{
			var refType = source?.GetType();
			return refType == null
				? $"FormatUtils.ReflectionFormat : {nameof(NullReferenceException)}"
				: $"[ReflectedType : {refType.Name}]\n" + string.Join("\n", refType.GetFields().Select(x => $"[{x.Name} : {x.GetValue(source)}]")) + "\n"
				  + string.Join("\n", refType.GetProperties().Select(x => $"[{x.Name} : {x.GetValue(source)}]"));
		}

		/// <summary>
		/// Compare the value to TEnum type
		/// </summary>
		/// <param name="value"></param>
		/// <typeparam name="TEnum"></typeparam>
		/// <returns></returns>
		public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct
		{
			Enum.TryParse(typeof(TEnum), value, out var result);
			if (result is null)
			{
				return default;
			}

			return (TEnum) result;
		}

		/// <summary>
		/// Compare the value to TEnum type
		/// </summary>
		/// <param name="value"></param>
		/// <param name="separator"></param>
		/// <typeparam name="TEnum"></typeparam>
		/// <returns></returns>
		public static TEnum[] ToEnums<TEnum>(this string value, string separator = ",") where TEnum : struct
		{
			if (string.IsNullOrEmpty(value))
				return Array.Empty<TEnum>();

			return value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
				.Select(v => v.ToEnum<TEnum>())
				.ToArray();
		}
	}
}