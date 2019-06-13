using Dawn;
using Helpers.MySql;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Generated;
using MovieTimes.CineworldService.Models.Helpers;
using MySql.Data.MySqlClient;
using OpenTracing;
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

		public Task SaveCinemasAsync(cinemas cinemas)
		{
			var (cinemaCount, filmCount, showCount) = cinemas.GetCounts();

			using (var scope = _tracer.BuildDefaultSpan()
				.WithTag(nameof(cinemaCount), cinemaCount)
				.WithTag(nameof(filmCount), filmCount)
				.WithTag(nameof(showCount), showCount)
				.StartActive(finishSpanOnDispose: true))
			{
				_logger.LogInformation("Saving {0:D} {1}(s), {2:D} {3}(s), {4:D} {5}(s)", cinemaCount, nameof(cinema), filmCount, nameof(film), showCount, nameof(show));

				using (var transaction = BeginTransaction())
				{
					try
					{
						ExecuteAsync("DELETE FROM cineworld.show WHERE cinemaId>=0");
						ExecuteAsync("DELETE FROM cineworld.film WHERE edi>=0");
						ExecuteAsync("DELETE FROM cineworld.cinema WHERE id>=0");

						ExecuteAsync(
							"INSERT cineworld.cinema(id, name) VALUES (@id, @name)",
							from c in cinemas.cinema
							select new { c.id, c.name, });

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
							});

						ExecuteAsync(
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
							});

						transaction.Commit();
					}
					catch (MySqlException ex)
					{
						scope.Span.Log(nameof(ex), ex.ToJsonString());
						_logger.LogCritical(ex, "Error saving cinemas");
						transaction.Rollback();
					}
				}

				return Task.CompletedTask;
			}
		}
	}
}
