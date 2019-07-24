using System;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface ICineworldRepository
	{
		Task<Models.LogEntry> GetLatestLogEntryAsync();
		Task LogAsync(Models.LogEntry logEntry);
		Task PurgeOldLogsAsync(DateTime? max = default);
		Task PurgeOldShowsAsync(DateTime? max = default);
		Task SaveCinemasAsync(Models.Generated.cinemas cinemas);
	}
}
