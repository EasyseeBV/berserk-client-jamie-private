using System;
using System.Text;

namespace RR.Core.Extensions
{
	public enum TimeFormat
	{
		Full = 0,
		MonthDayHour = 1,
		DayHour = 2,
		HourMinuteSecond = 3,
	}

	public static class TimeExtensions
	{
		public static bool IsBetween(this DateTime currentDateUtc, DateTime utcTime1, DateTime utcTime2)
		{
			var min = utcTime1 < utcTime2 ? utcTime1 : utcTime2;
			var max = utcTime1 > utcTime2 ? utcTime1 : utcTime2;
			return currentDateUtc >= min && currentDateUtc <= max;
		}

		public static bool IsEarlierThan(this DateTime sourceUtcTime, DateTime targetUtcTime)
		{
			return sourceUtcTime < targetUtcTime;
		}

		public static bool IsLaterThan(this DateTime sourceUtcTime, DateTime targetUtcTime)
		{
			return sourceUtcTime > targetUtcTime;
		}

		public static bool IsEarlierThanNow(this DateTime timeUtc)
		{
			return timeUtc < DateTime.UtcNow;
		}

		public static TimeSpan GetTimeTillNow(this DateTime timeUtc)
		{
			return timeUtc.Subtract(DateTime.UtcNow);
		}

		public static string ToString(this TimeSpan input, TimeFormat timeFormat)
		{
			switch (timeFormat)
			{
				case TimeFormat.MonthDayHour: return $"{input:MM\\:dd\\:hh}";
				case TimeFormat.DayHour: return $"{input:dd\\:hh}";
				case TimeFormat.HourMinuteSecond: return $"{input:hh\\:mm\\:ss}";
				default: return $"{input:dd\\:hh\\:mm\\:ss}";
			}
		}

		public static string ToString(this DateTime input, TimeFormat timeFormat = TimeFormat.Full)
		{
			switch (timeFormat)
			{
				case TimeFormat.MonthDayHour: return $"{input:MM\\:dd\\:hh}";
				case TimeFormat.DayHour: return $"{input:dd\\:hh}";
				case TimeFormat.HourMinuteSecond: return $"{input:hh\\:mm\\:ss}";
				default: return $"{input:dd\\:hh\\:mm\\:ss}";
			}
		}

		public static string ToShortString(this TimeSpan time)
		{
			var formatSb = new StringBuilder("mm\\:ss", 20);

			if (time.Hours > 0)
				formatSb.Insert(0, "hh\\:");

			if (time.Days > 0)
				formatSb.Insert(0, "dd\\:");

			var format = formatSb.ToString();

			return time.ToString(format);
		}
	}
}