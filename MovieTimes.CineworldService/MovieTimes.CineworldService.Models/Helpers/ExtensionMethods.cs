using MovieTimes.CineworldService.Models.Generated;
using System;
using System.Linq;
using System.Text.Json;

namespace MovieTimes.CineworldService.Models.Helpers
{
	public static class ExtensionMethods
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			IgnoreNullValues = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true,
		};

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
				return JsonSerializer.Serialize(value, _jsonSerializerOptions);
			}
			catch
			{
				return JsonSerializer.Serialize(new { value = value.ToString(), }, _jsonSerializerOptions);
			}
		}
	}
}
