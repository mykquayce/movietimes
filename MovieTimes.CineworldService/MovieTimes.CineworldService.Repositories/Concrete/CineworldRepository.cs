using Dapper;
using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
	public class CineworldRepository : ICineworldRepository
	{
		private readonly ILogger<CineworldRepository> _logger;
		private readonly ITracer _tracer;
		private IDbConnection _connection;
		private readonly string _connectionString;

		public CineworldRepository(
			ILogger<CineworldRepository> logger,
			ITracer tracer,
			IOptions<Configuration.DbSettings> dbSettingsOptions,
			IOptions<Configuration.DockerSecrets> dockerSecretsOptions)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;
			_tracer = Guard.Argument(() => tracer).NotNull().Value;

			Guard.Argument(() => dbSettingsOptions, secure: true).NotNull();
			var dbSettings = Guard.Argument(() => dbSettingsOptions.Value, secure: true).NotNull().Value;

			Guard.Argument(() => dbSettings, secure: true)
				.NotNull()
				.Require(s => !string.IsNullOrWhiteSpace(s.Server))
				.Require(s => s.Port > 0)
				.Require(s => !string.IsNullOrWhiteSpace(s.Database));

			Guard.Argument(() => dockerSecretsOptions, secure: true).NotNull();

			var dockerSecrets = dockerSecretsOptions.Value;

			var server = dbSettings.Server;
			var port = dbSettings.Port;
			var userId = dockerSecrets.MySqlCineworldUser ?? dbSettings.UserId;
			var password = dockerSecrets.MySqlCineworldPassword ?? dbSettings.Password;
			var database = dbSettings.Database;

			Guard.Argument(() => userId, secure: true).NotNull().NotEmpty().NotWhiteSpace();
			Guard.Argument(() => password, secure: true).NotNull().NotEmpty().NotWhiteSpace();

			_connectionString = $"server={server};port={port:D};user id={userId};password={password};database={database};";

			Connect();

			Deploy();
		}

		private bool Connected => _connection != default && (_connection.State & ConnectionState.Open) != 0;

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

				CheckConnection();

				using (var transaction = _connection.BeginTransaction())
				{
					Task<int> Do(string sql, object param = default)
					{
						return _connection.ExecuteAsync(sql, param, transaction, commandTimeout: 60, commandType: CommandType.Text);
					}

					try
					{
						Do("DELETE FROM cineworld.show WHERE cinemaId>=0");
						Do("DELETE FROM cineworld.film WHERE edi>=0");
						Do("DELETE FROM cineworld.cinema WHERE id>=0");

						Do(
							"INSERT cineworld.cinema(id, name) VALUES (@id, @name)",
							from c in cinemas.cinema
							select new { c.id, c.name, });

						Do(
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

						Do(
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

		private void CheckConnection()
		{
			using (var scope = _tracer.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true))
			{
				try
				{
					var now = _connection.ExecuteScalar("SELECT NOW();");
				}
				catch (MySqlException ex)
					when (string.Equals(ex.Message, "Fatal error encountered during command execution.", StringComparison.InvariantCultureIgnoreCase)
						&& ex?.InnerException is System.IO.IOException
						&& string.Equals(ex.InnerException.Message, "Unable to write data to the transport connection: Connection reset by peer.", StringComparison.InvariantCultureIgnoreCase))
				{
					Connect();
				}
			}
		}

		private void Connect()
		{
			using (var scope = _tracer.BuildDefaultSpan()
				.WithTag(nameof(Connected), Connected)
				.StartActive(finishSpanOnDispose: true))
			{
				if (_connection == null)
				{
					_connection = new MySqlConnection(_connectionString);
				}

				var count = 0;

				do
				{

					_logger.LogInformation($"Current DB connection state: {_connection.State:F}");

					if (Connected)
					{
						_logger.LogInformation($"Connected to DB after {count:D} attempt(s)");
						return;
					}

					_logger.LogInformation($"Connecting to DB: attempt {count:D}");
					scope.Span.Log(
						"attempt" + (count + 1), _connection.State);

					_connection.Open();

					System.Threading.Thread.Sleep(millisecondsTimeout: 3_000);
				}
				while (++count <= 10);

				var exception = new Exception("Failed to connect to DB");
				_logger.LogCritical(exception, exception.Message);
				scope.Span.Log(
					nameof(exception), exception.ToJsonString());
				throw exception;
			}
		}

		private Task Deploy()
		{
			_logger.LogInformation("Deploying DB");

			return Task.WhenAll(
				DeployTable(nameof(Properties.Resources.cinema), Properties.Resources.cinema),
				DeployTable(nameof(Properties.Resources.film),   Properties.Resources.film),
				DeployTable(nameof(Properties.Resources.show),   Properties.Resources.show));
		}

		private async Task DeployTable(string tableName, string sql)
		{
			var result = await _connection.ExecuteScalarAsync("SHOW TABLES FROM cineworld LIKE @tableName", new { tableName, });

			if (result as string == tableName)
			{
				return;
			}

			await _connection.ExecuteAsync(sql);
		}
	}
}
