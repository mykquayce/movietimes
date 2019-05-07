using Dapper;
using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTimes.Api.Models;
using MySql.Data.MySqlClient;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieTimes.Api.Repositories.Concrete
{
	public class CineworldRepository : ICineworldRepository, IDisposable
	{
		private readonly ITracer _tracer;
		private readonly ILogger _logger;
		private IDbConnection _connection;
		private readonly string _connectionString;

		public CineworldRepository(
			ITracer tracer,
			ILogger<CineworldRepository> logger,
			IOptions<Configuration.DbSettings> dbSettingsOptions,
			IOptions<Configuration.DockerSecrets> dockerSecretsOptions)
		{
			_tracer = tracer;
			_logger = logger;

			var dbSettings = dbSettingsOptions.Value;
			var dockerSecrets = dockerSecretsOptions?.Value;

			var server = dbSettings.Server;
			var port = dbSettings.Port;
			var userId = dockerSecrets?.MySqlCineworldUser ?? dbSettings.UserId;
			var password = dockerSecrets?.MySqlCineworldPassword ?? dbSettings.Password;
			var database = dbSettings.Database;

			_connectionString = $"server={server};port={port:D};user id={userId};password={password};database={database};";

			Connect();
		}

		public void Dispose()
		{
			_connection?.Dispose();
		}

		public async Task<IEnumerable<(short id, string name)>> GetCinemasAsync(ICollection<string> searchTerms)
		{
			var searchTermsString = string.Join(", ", searchTerms);

			using (var scope = _tracer.BuildDefaultSpan()
				.WithTag(nameof(searchTerms), searchTermsString)
				.StartActive(finishSpanOnDispose: true))
			{
				_logger?.LogInformation($"{nameof(searchTerms)}={searchTermsString}");

				var tasks = searchTerms.Select(GetCinemasAsync);

				var cinemases = await Task.WhenAll(tasks);

				return from tuples in cinemases
					   from tuple in tuples
					   select tuple;
			}
		}

		public Task<IEnumerable<(short id, string name)>> GetCinemasAsync(string search = default)
		{
			using (var scope = _tracer?.BuildDefaultSpan()
				.WithTag(nameof(search), search)
				.StartActive(finishSpanOnDispose: true))
			{
				_logger?.LogInformation($"{nameof(search)}={search}");

				CheckConnection();

				if (string.IsNullOrWhiteSpace(search))
				{
					return _connection.QueryAsync<(short id, string name)>(
						"SELECT * FROM cineworld.cinema;");
				}

				return _connection.QueryAsync<(short id, string name)>(
					"SELECT * FROM cineworld.cinema WHERE name LIKE @search;",
					new { search = $"%{search}%", });
			}
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

			using (var scope = _tracer?.BuildDefaultSpan()
				.WithTag(nameof(cinemaIds), cinemaIdsString)
				.WithTag(nameof(daysOfWeek), daysOfWeekString)
				.WithTag(nameof(timesOfDay), timesOfDayString)
				.WithTag(nameof(searchTerms), searchTermsStrings)
				.StartActive(finishSpanOnDispose: true))
			{
				CheckConnection();

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

				var results = await _connection.QueryAsync<(short cinemaId, string cinemaName, DateTime dateTime, string title)>(sql, @params);

				return results;
			}
		}

		public void Connect()
		{
			_connection = new MySqlConnection(_connectionString);
		}

		public void CheckConnection()
		{
			if (_connection == default)
			{
				Connect();
			}

			try
			{
				var now = _connection.ExecuteScalar("SELECT NOW();");
			}
			catch (MySqlException)
			{
				Connect();
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
