using MovieTimes.CineworldService.Models.Generated;
using System;
using System.Linq;

namespace MovieTimes.CineworldService.Models.Helpers
{
	public static class ExtensionMethods
	{
		public static (int cinemaCount, int filmCount, int showCount) GetCounts(this cinemas cinemas)
		{
			return (
				cinemas?.cinema?.Count ?? 0,
				cinemas?.cinema?.Sum(c => c?.listing?.Count ?? 0) ?? 0,
				cinemas?.cinema?.Sum(c => c?.listing?.Sum(s => s?.shows?.Count ?? 0) ?? 0) ?? 0);
		}

		public static string Truncate(this string s) => s?.Substring(0, Math.Min(s.Length, 100)) ?? string.Empty;
	}
}
