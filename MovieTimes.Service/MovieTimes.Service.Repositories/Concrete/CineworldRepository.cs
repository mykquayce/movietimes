using Microsoft.Extensions.Options;
using MovieTimes.Service.Models;
using MovieTimes.Service.Models.Generated;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories.Concrete
{
	public class CineworldRepository : Helpers.MySql.RepositoryBase, ICineworldRepository
	{
		public CineworldRepository(IOptions<Helpers.MySql.Models.DbSettings> options)
			: base(options.Value)
		{ }

		public async Task<LogEntry?> GetLatestLogEntryAsync()
		{
			var sql = "SELECT `lastModified` FROM `cineworld`.`log` ORDER BY `lastModified` DESC LIMIT 1;";

			await foreach (var logEntry in base.QueryAsync<LogEntry>(sql))
			{
				return logEntry;
			}

			return default;
		}

		public Task LogAsync(LogEntry logEntry)
		{
			if (logEntry == default) throw new ArgumentNullException(nameof(logEntry));
			if (!logEntry.LastModified.HasValue) throw new ArgumentNullException(nameof(LogEntry.LastModified));
			if (logEntry.LastModified.Value.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentOutOfRangeException(nameof(LogEntry.LastModified), logEntry.LastModified, nameof(LogEntry.LastModified) + " must be UTC")
				{
					Data = { [nameof(LogEntry.LastModified)] = logEntry.LastModified, },
				};
			}

			return base.ExecuteAsync(
				"INSERT `cineworld`.`log` (`lastModified`) VALUES (@lastModified);",
				logEntry);
		}

		public Task PurgeOldLogsAsync(DateTime? max = default) =>
			base.ExecuteAsync("DELETE FROM `cineworld`.`log` WHERE `lastModified` <= @max;", new { max = max ?? DateTime.UtcNow, });

		public Task PurgeOldShowsAsync(DateTime? max = null) =>
			base.ExecuteAsync("DELETE FROM `cineworld`.`show` WHERE `time` <= @max;", new { max = max ?? DateTime.UtcNow, });

		public async Task SaveCinemasAsync(cinemas cinemas)
		{
			using var transaction = base.BeginTransaction();

			try
			{
				await Task.WhenAll(
					base.ExecuteAsync(
						"INSERT IGNORE INTO `cineworld`.`cinema` (`id`, `name`) VALUES (@id, @name);",
						from c in cinemas.cinema
						select new
						{
							c.id,
							c.name,
						},
						transaction: transaction),
					base.ExecuteAsync(
						"INSERT IGNORE INTO `cineworld`.`film` (`edi`, `title`) VALUES (@edi, @title)",
						from c in cinemas.cinema
						from f in c.listing
						where f.edi > 0 // Theatre Let
						group f by f.edi into gg
						select new
						{
							edi = gg.Key,
							gg.First().title,
						},
						transaction: transaction)
				);

				await base.ExecuteAsync(
					"INSERT IGNORE INTO `cineworld`.`show` (`cinemaId`, `filmEdi`, `time`) VALUES (@cinemaId, @filmEdi, @time)",
					from c in cinemas.cinema
					from f in c.listing
					where f.edi > 0 // Theatre Let
					from s in f.shows
					group (c, f, s) by (c.id, f.edi, s.time) into gg
					select new
					{
						cinemaId = gg.Key.id,
						filmEdi = gg.Key.edi,
						gg.Key.time,
					},
					transaction: transaction);

				transaction.Commit();
			}
			catch (MySqlException)
			{
				transaction.Rollback();
				throw;
			}
		}
	}
}
