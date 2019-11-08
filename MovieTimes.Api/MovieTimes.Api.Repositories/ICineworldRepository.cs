using MovieTimes.Api.Models;
using MovieTimes.Api.Models.Enums;
using System;
using System.Collections.Generic;

namespace MovieTimes.Api.Repositories
{
	public interface ICineworldRepository
	{
		IAsyncEnumerable<Cinema> GetCinemasAsync(ICollection<string> names);

		IAsyncEnumerable<Cinema> GetCinemasAsync(string? search = default)
			=> GetCinemasAsync(string.IsNullOrWhiteSpace(search) ? Array.Empty<string>() : new[] { search!, });

		IAsyncEnumerable<Show> GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms,
			int weekCount);
	}
}
