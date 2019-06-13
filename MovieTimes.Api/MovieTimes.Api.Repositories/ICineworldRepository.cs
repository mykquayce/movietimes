using MovieTimes.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Api.Repositories
{
	public interface ICineworldRepository
	{
		Task<IEnumerable<(short id, string name)>> GetCinemasAsync(ICollection<string> names);

		Task<IEnumerable<(short id, string name)>> GetCinemasAsync(string? search = default);

		Task<IEnumerable<(short cinemaId, string cinemaName, DateTime dateTime, string title)>> GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms);
	}
}
