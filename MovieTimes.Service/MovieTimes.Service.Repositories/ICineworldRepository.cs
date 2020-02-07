using Helpers.Cineworld.Models;
using Helpers.Cineworld.Models.Generated;
using MovieTimes.Service.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface ICineworldRepository : IDisposable
	{
		Task<Models.LogEntry?> GetLatestLogEntryAsync();
		Task LogAsync(Models.LogEntry logEntry);
		Task PurgeOldLogsAsync(DateTime? max = default);
		Task PurgeOldShowsAsync(DateTime? max = default);
		IAsyncEnumerable<QueryResult> RunQueryAsync(Query query);
		Task SaveCinemasAsync(ICollection<CinemaType> cinemas);
		Task SaveLengthsAsync(ICollection<FilmType> films);
	}
}
