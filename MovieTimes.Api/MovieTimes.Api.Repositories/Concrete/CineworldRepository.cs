using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTimes.Api.Models.Enums;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieTimes.Api.Repositories.Concrete
{
	public class CineworldRepository : Helpers.MySql.RepositoryBase, ICineworldRepository
	{
		private readonly ITracer? _tracer;
		private readonly ILogger? _logger;

		public CineworldRepository(
			ITracer? tracer,
			ILogger<CineworldRepository>? logger,
			IOptions<Helpers.MySql.Models.DbSettings> options)
			: base(options)
		{
			_tracer = tracer;
			_logger = logger;
		}

		public async IAsyncEnumerable<Helpers.Cineworld.Models.cinemaType> GetCinemasAsync(ICollection<string> searchTerms)
		{
			var searchTermsString = string.Join(", ", searchTerms);

			using var _ = _tracer?.BuildDefaultSpan()
				.WithTag(nameof(searchTerms), searchTermsString)
				.StartActive(finishSpanOnDispose: true);

			_logger?.LogInformation($"{nameof(searchTerms)}={searchTermsString}");

			if (searchTerms.Count == 0)
			{
				searchTerms.Add(string.Empty);
			}

			foreach (var searchTerm in searchTerms)
			{
				await foreach (var cinema in GetCinemasAsync(searchTerm))
				{
					yield return cinema;
				}
			}
		}

		public IAsyncEnumerable<Helpers.Cineworld.Models.cinemaType> GetCinemasAsync(string? search = default)
		{
			_logger?.LogInformation($"{nameof(search)}={search}");

			if (string.IsNullOrWhiteSpace(search))
			{
				return base.QueryAsync<Helpers.Cineworld.Models.cinemaType>("SELECT * FROM cineworld.cinema;");
			}

			return base.QueryAsync<Helpers.Cineworld.Models.cinemaType>(
				"SELECT * FROM cineworld.cinema WHERE name LIKE @search;",
				new { search = $"%{search}%", });
		}

		public async IAsyncEnumerable<Helpers.Cineworld.Models.CinemaMovieShow> GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms,
			int weekCount)
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

			var dateRangeSql = weekCount <= 0
				? "1 = 1"
				: $"s.time BETWEEN CURRENT_DATE() AND DATE_ADD(CURRENT_DATE(), INTERVAL {weekCount:D} WEEK)";

			var sql = $@"SELECT c.name cinemaName, s.time, f.title, f.duration
					FROM cineworld.cinema c
						JOIN cineworld.show s ON c.id = s.cinemaId
						JOIN cineworld.film f ON s.filmEdi = f.edi
					WHERE {cinemaIdsSql}
						AND {dayNameSql}
						AND {SqlFromTimesOfDay(timesOfDay)}
						AND {SqlFromSearchTerms(searchTerms)}
						AND {dateRangeSql}
						ORDER BY c.name, s.time, f.title;";

			var @params = new
			{
				cinemaIds,
				daysOfWeek = daysOfWeekString.Split(','),
			};

			var enumerable = base.QueryAsync<(string cinemaName, DateTime dateTime, string title, short duration)>(sql, @params);

			await foreach ((string cinemaName, DateTime dateTime, string title, short duration) in enumerable)
			{
				yield return new Helpers.Cineworld.Models.CinemaMovieShow
				{
					Cinema = cinemaName,
					DateTime = dateTime,
					Movie = title,
					End = dateTime.TimeOfDay + TimeSpan.FromMinutes(duration) + TimeSpan.FromMinutes(30),
				};
			}
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
