using MovieTimes.CineworldService.Models.Generated;
using Newtonsoft.Json;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MovieTimes.CineworldService.Models.Helpers
{
	public static class ExtensionMethods
	{
		private const string _solutionName = "MovieTimes.CineworldService";

		public static (int cinemaCount, int filmCount, int showCount) GetCounts(this cinemas cinemas)
		{
			return (
				cinemas?.cinema?.Count ?? 0,
				cinemas?.cinema?.Sum(c => c?.listing?.Count ?? 0) ?? 0,
				cinemas?.cinema?.Sum(c => c?.listing?.Sum(s => s?.shows?.Count ?? 0) ?? 0) ?? 0);
		}

		public static string Truncate(this string s) => s?.Substring(0, Math.Min(s.Length, 100)) ?? string.Empty;

		public static string ToJsonString(this object value)
		{
			if (value == default)
			{
				return string.Empty;
			}

			try
			{
				return JsonConvert.SerializeObject(value);
			}
			catch
			{
				return JsonConvert.SerializeObject(new { value = value.ToString(), });
			}
		}

		private static string ReducePath(this string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}

			var segments = path.Split(Path.DirectorySeparatorChar);

			if (segments.Length == 1)
			{
				return segments[0];
			}

			var query = segments
				.SkipWhile(s => !string.Equals(s, _solutionName, StringComparison.InvariantCultureIgnoreCase))
				.Skip(1)
				.ToList();

			if (query.Count == 0)
			{
				return path;
			}

			return string.Join(Path.DirectorySeparatorChar.ToString(), query);
		}

		public static ISpanBuilder BuildDefaultSpan(
			this ITracer tracer,
			[CallerFilePath] string filePath = default,
			[CallerMemberName] string methodName = default)
		{
			var path = filePath.ReducePath();

			var name = string.Concat(path, ".", methodName);

			return tracer.BuildSpan(name);
		}

		public static ISpan Log(this ISpan span, params object[] values)
		{
			var dictionary = new Dictionary<string, object>(values.Length / 2);

			for (var a = 0; a < values.Length; a += 2)
			{
				var key = values[a].ToString();
				var value = values[a + 1];

				dictionary[key] = value;
			}

			span.Log(dictionary);

			return span;
		}
	}
}
