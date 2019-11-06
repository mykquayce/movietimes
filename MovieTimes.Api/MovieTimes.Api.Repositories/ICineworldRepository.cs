using MovieTimes.Api.Models.Enums;
using System;
using System.Collections.Generic;

namespace MovieTimes.Api.Repositories
{
	public interface ICineworldRepository
	{
		IAsyncEnumerable<(short id, string name)> GetCinemasAsync(ICollection<string> names);

		IAsyncEnumerable<(short id, string name)> GetCinemasAsync(string? search = default);

		IAsyncEnumerable<(short cinemaId, string cinemaName, DateTime dateTime, string title, short duration)> GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms,
			int weekCount);
	}
}
