using Helpers.Cineworld.Models.Enums;
using System;
using System.Collections.Generic;

namespace MovieTimes.Service.Models
{
	public static class ExtensionMethods
	{
		public static IEnumerable<(object key, object value)> Data(this Exception exception)
		{
			var enumerator = exception.Data.GetEnumerator();

			while (enumerator.MoveNext())
			{
				yield return (enumerator.Key, enumerator.Value);
			}

			if (exception.InnerException is null)
			{
				yield break;
			}

			foreach (var kvp in Data(exception.InnerException))
			{
				yield return kvp;
			}
		}

		public static string Message(this Exception exception)
		{
			if (exception.InnerException is null)
			{
				return exception.Message;
			}

			return exception.InnerException.Message();
		}

		public static DaysOfWeek ToDaysOfWeek(this DayOfWeek dayOfWeek)
		{
			return dayOfWeek switch
			{
				DayOfWeek.Sunday => DaysOfWeek.Sunday,
				DayOfWeek.Monday => DaysOfWeek.Monday,
				DayOfWeek.Tuesday => DaysOfWeek.Tuesday,
				DayOfWeek.Wednesday => DaysOfWeek.Wednesday,
				DayOfWeek.Thursday => DaysOfWeek.Thursday,
				DayOfWeek.Friday => DaysOfWeek.Friday,
				DayOfWeek.Saturday => DaysOfWeek.Saturday,
				_ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, $"Unexpected value of {nameof(dayOfWeek)}: {dayOfWeek:G}"),
			};
		}
	}
}
