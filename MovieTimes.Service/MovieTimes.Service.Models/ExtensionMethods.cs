using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieTimes.Service.Models
{
	public static class ExtensionMethods
	{
		public static IEnumerable<(object key, object value)> Data(this Exception exception)
		{
			foreach (var key in exception.Data.Keys)
			{
				var value = exception.Data[key];

				yield return (key, value);
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
	}
}
