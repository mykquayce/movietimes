using MovieTimes.Api.Models.Enums;
using System;
using System.Collections.Generic;

namespace MovieTimes.Api.Repositories
{
	public interface ICineworldRepository
	{
		IAsyncEnumerable<Helpers.Cineworld.Models.Generated.CinemaType> GetCinemasAsync(ICollection<string> names);

		IAsyncEnumerable<Helpers.Cineworld.Models.Generated.CinemaType> GetCinemasAsync(string? search = default)
			=> GetCinemasAsync(string.IsNullOrWhiteSpace(search) ? Array.Empty<string>() : new[] { search!, });

		IAsyncEnumerable<Models.Flattened> GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms,
			int weekCount);
	}
}
