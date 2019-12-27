using Helpers.Cineworld.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface ICineworldRepository
	{
		Task<Models.LogEntry?> GetLatestLogEntryAsync();
		Task LogAsync(Models.LogEntry logEntry);
		Task PurgeOldLogsAsync(DateTime? max = default);
		Task PurgeOldShowsAsync(DateTime? max = default);
		IAsyncEnumerable<cinemaType> RunQueryAsync(Query query);
		Task SaveCinemasAsync(cinemasType cinemas);
	}
}
