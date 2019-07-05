using Dawn;
using Helpers.MySql;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Generated;
using MovieTimes.CineworldService.Models.Helpers;
using MySql.Data.MySqlClient;
using OpenTracing;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Repositories.Concrete
{
	public class CineworldRepository : RepositoryBase, ICineworldRepository
	{
		private readonly ILogger _logger;
		private readonly ITracer _tracer;

		public CineworldRepository(
			string connectionString,
			ILogger<CineworldRepository> logger,
			ITracer tracer)
			: base(connectionString, logger)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
		}

		public new ConnectionState ConnectionState => base.ConnectionState;
		public new void Connect() => base.Connect();
		public Task<DateTime> GetDateTimeAsync() => ExecuteScalarAsync<DateTime>("SELECT NOW();");

		public async Task<DateTime?> GetLastModifiedFromLogAsync()
		{
			using var scope = _tracer.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			_logger.LogInformation(nameof(GetLastModifiedFromLogAsync));

			var lastModified = await ExecuteScalarAsync<DateTime?>("SELECT `lastModified` FROM `cineworld`.`log` ORDER BY `lastModified` DESC LIMIT 1;");

			if (lastModified.HasValue)
			{
				lastModified = DateTime.SpecifyKind(lastModified.Value, DateTimeKind.Utc);
			}

			scope.Span.Log(nameof(lastModified), lastModified);

			_logger.LogInformation($"{nameof(GetLastModifiedFromLogAsync)}: {lastModified}");

			return lastModified;
		}

		public async Task LogAsync(DateTime lastModified)
		{
			Guard.Argument(() => lastModified)
				.NotDefault()
				.Require(dt => dt.Kind == DateTimeKind.Utc, dt => $"{nameof(lastModified)} must be {nameof(DateTimeKind.Utc)}")
				.InRange(DateTime.UtcNow.Date.AddDays(-3), DateTime.UtcNow);

			using var scope = _tracer.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			scope.Span.Log(nameof(lastModified), lastModified);

			_logger.LogInformation($"{nameof(LogAsync)}: {lastModified:O}");

			try
			{
				await ExecuteAsync("INSERT `cineworld`.`log` (lastModified) VALUES(@lastModified);", new { lastModified, });
			}
			catch (Exception ex)
			{
				ex.Data.Add(nameof(lastModified), lastModified);

				throw;
			}
		}

		public async Task SaveCinemasAsync(cinemas cinemas)
		{
			var (cinemaCount, filmCount, showCount) = cinemas.GetCounts();

			using var scope = _tracer.BuildDefaultSpan()
				.WithTag(nameof(cinemaCount), cinemaCount)
				.WithTag(nameof(filmCount), filmCount)
				.WithTag(nameof(showCount), showCount)
				.StartActive(finishSpanOnDispose: true);

			await GetDateTimeAsync();

			_logger.LogInformation("Saving {0:D} {1}(s), {2:D} {3}(s), {4:D} {5}(s)", cinemaCount, nameof(cinema), filmCount, nameof(film), showCount, nameof(show));

			using var transaction = BeginTransaction();

			try
			{
				await ExecuteAsync("DELETE FROM cineworld.show WHERE cinemaId>=0", transaction: transaction);

				await Task.WhenAll(
					ExecuteAsync("DELETE FROM cineworld.film WHERE edi>=0", transaction: transaction),
					ExecuteAsync("DELETE FROM cineworld.cinema WHERE id>=0", transaction: transaction)
					);

				await Task.WhenAll(
					ExecuteAsync(
						"INSERT cineworld.cinema(id, name) VALUES (@id, @name)",
						from c in cinemas.cinema
						select new
						{
							c.id,
							c.name,
						},
						transaction: transaction),
					ExecuteAsync(
						"INSERT cineworld.film(edi, title) VALUES (@edi, @title)",
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

				await ExecuteAsync(
					"INSERT cineworld.show(cinemaId, filmEdi, time) VALUES (@cinemaId, @filmEdi, @time)",
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
			catch (MySqlException ex)
			{
				scope.Span.Log(nameof(ex), ex.ToJsonString());
				_logger.LogCritical(ex, "Error saving cinemas");
				transaction.Rollback();
			}
		}
	}
}
