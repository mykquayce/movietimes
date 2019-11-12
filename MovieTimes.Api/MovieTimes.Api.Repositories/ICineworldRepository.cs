using MovieTimes.Api.Models.Enums;
using System;
using System.Collections.Generic;

namespace MovieTimes.Api.Repositories
{
	public interface ICineworldRepository
	{
		IAsyncEnumerable<Helpers.Cineworld.Models.cinemaType> GetCinemasAsync(ICollection<string> names);

		IAsyncEnumerable<Helpers.Cineworld.Models.cinemaType> GetCinemasAsync(string? search = default)
			=> GetCinemasAsync(string.IsNullOrWhiteSpace(search) ? Array.Empty<string>() : new[] { search!, });

		IAsyncEnumerable<Helpers.Cineworld.Models.CinemaMovieShow> GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms,
			int weekCount);
	}
}
