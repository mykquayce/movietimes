using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using MovieTimes.Api.Models;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.Api.Repositories.Concrete
{
	public class CineworldRepository : Helpers.MySql.RepositoryBase, ICineworldRepository
	{
		private readonly ITracer? _tracer;
		private readonly ILogger? _logger;

		public CineworldRepository(
			ITracer? tracer,
			ILogger<CineworldRepository>? logger,
			string connectionString)
			: base(connectionString, logger)
		{
			_tracer = tracer;
			_logger = logger;
		}

		public async Task<IEnumerable<(short id, string name)>> GetCinemasAsync(ICollection<string> searchTerms)
		{
			var searchTermsString = string.Join(", ", searchTerms);

			using var _ = _tracer.BuildDefaultSpan()
				.WithTag(nameof(searchTerms), searchTermsString)
				.StartActive(finishSpanOnDispose: true);

			_logger?.LogInformation($"{nameof(searchTerms)}={searchTermsString}");

			var tasks = searchTerms.Select(GetCinemasAsync);

			var cinemases = await Task.WhenAll(tasks);

			return from tuples in cinemases
				   from tuple in tuples
				   select tuple;
		}

		public async Task<IEnumerable<(short id, string name)>> GetCinemasAsync(string? search = default)
		{
			using var _ = _tracer?.BuildDefaultSpan()
				.WithTag(nameof(search), search)
				.StartActive(finishSpanOnDispose: true);

			_logger?.LogInformation($"{nameof(search)}={search}");

			if (string.IsNullOrWhiteSpace(search))
			{
				return await base.QueryAsync<(short id, string name)>(
					"SELECT * FROM cineworld.cinema;");
			}

			return await base.QueryAsync<(short id, string name)>(
				"SELECT * FROM cineworld.cinema WHERE name LIKE @search;",
				new { search = $"%{search}%", });
		}

		public async Task<IEnumerable<(short cinemaId, string cinemaName, DateTime dateTime, string title)>> GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms)
		{
			Guard.Argument(() => cinemaIds).NotNull();
			Guard.Argument(() => searchTerms).NotNull();

			var cinemaIdsString = string.Join(", ", cinemaIds);
			var daysOfWeekString = daysOfWeek.ToString("F");
			var timesOfDayString = timesOfDay.ToString("F");
			var searchTermsStrings = string.Join(", ", searchTerms);

			_logger?.LogInformation($"{nameof(cinemaIds)}={cinemaIdsString},{nameof(daysOfWeek)}={daysOfWeekString},{nameof(timesOfDay)}={timesOfDayString},{nameof(searchTerms)}={searchTermsStrings}");

			using var _ = _tracer?.BuildDefaultSpan()
				.WithTag(nameof(cinemaIds), cinemaIdsString)
				.WithTag(nameof(daysOfWeek), daysOfWeekString)
				.WithTag(nameof(timesOfDay), timesOfDayString)
				.WithTag(nameof(searchTerms), searchTermsStrings)
				.StartActive(finishSpanOnDispose: true);

			var cinemaIdsSql = cinemaIds.Count == 0
				? "1 = 1"
				: "c.id IN @cinemaIds";

			var dayNameSql = daysOfWeek == DaysOfWeek.None
				? "1 = 1"
				: "DAYNAME(s.time) IN @daysOfWeek";

			var sql = $@"SELECT c.id cinemaId, c.name cinemaName, s.time, f.title
					FROM cineworld.cinema c
						JOIN cineworld.show s ON c.id = s.cinemaId
						JOIN cineworld.film f ON s.filmEdi = f.edi
					WHERE {cinemaIdsSql}
						AND {dayNameSql}
						AND {SqlFromTimesOfDay(timesOfDay)}
						AND {SqlFromSearchTerms(searchTerms)}
						ORDER BY c.name, s.time, f.title;";

			var @params = new
			{
				cinemaIds,
				daysOfWeek = daysOfWeekString.Split(','),
			};

			var results = await base.QueryAsync<(short cinemaId, string cinemaName, DateTime dateTime, string title)>(sql, @params);

			return results;
		}

		private static string SqlFromTimesOfDay(TimesOfDay timesOfDay)
		{
			var list = new List<string>();

			if ((timesOfDay & TimesOfDay.Night) != 0)
			{
				list.Add("(TIME(s.time) >= '00:00:00' AND TIME(s.time) <= '06:00:00')");
			}

			if ((timesOfDay & TimesOfDay.Morning) != 0)
			{
				list.Add("(TIME(s.time) >= '06:00:00' AND TIME(s.time) <= '12:00:00')");
			}

			if ((timesOfDay & TimesOfDay.Afternoon) != 0)
			{
				list.Add("(TIME(s.time) >= '12:00:00' AND TIME(s.time) <= '18:00:00')");
			}

			if ((timesOfDay & TimesOfDay.Evening) != 0)
			{
				list.Add("((TIME(s.time) >= '18:00:00' AND TIME(s.time) <= '24:00:00') OR TIME(s.time) = '00:00:00')");
			}

			if (list.Count == 0)
			{
				return "1 = 1";
			}

			return string.Concat("(", string.Join(" OR ", list), ")");
		}

		private static string SqlFromSearchTerms(ICollection<string> searchTerms)
		{
			if (searchTerms.Count == 0)
			{
				return "1 = 1";
			}

			var list = searchTerms.Select(t => $"f.title LIKE '%{t}%'");
			var joined = string.Join(" OR ", list);

			return string.Concat("(", joined, ")");
		}
	}
}
