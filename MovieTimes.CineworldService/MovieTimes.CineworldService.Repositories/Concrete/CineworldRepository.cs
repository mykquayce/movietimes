using Dapper;
using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTimes.CineworldService.Models.Generated;
using MovieTimes.CineworldService.Models.Helpers;
using MySql.Data.MySqlClient;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Repositories.Concrete
{
	public class CineworldRepository : ICineworldRepository
	{
		private readonly ILogger<CineworldRepository> _logger;
		private readonly ITracer _tracer;
		private readonly IDbConnection _connection;

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

			var connectionString = $"server={server};port={port:D};user id={userId};password={password};database={database};";

			_connection = new MySqlConnection(connectionString);

			Connect();

			if (!Connected)
			{
				return;
			}

			Deploy();
		}

		private bool Connected => (_connection.State & ConnectionState.Open) != 0;

		public Task SaveCinemasAsync(cinemas cinemas)
		{
			var (cinemaCount, filmCount, showCount) = cinemas.GetCounts();

			_logger.LogInformation("Saving {0:D} {1}(s), {2:D} {3}(s), {4:D} {5}(s)", cinemaCount, nameof(cinema), filmCount, nameof(film), showCount, nameof(show));

			Connect();

			if (!Connected)
			{
				_logger.LogCritical("DB was inaccessible");
				return Task.CompletedTask;
			}

			IDbTransaction transaction;

			using (transaction = _connection.BeginTransaction())
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
					_logger.LogCritical(ex, "Error saving cinemas");
					transaction.Rollback();
				}
			}

			return Task.CompletedTask;
		}

		private void Connect()
		{
			using (var scope = _tracer.BuildSpan($"{nameof(CineworldRepository)}.{nameof(Connect)}")
				.WithTag(nameof(Connected), Connected)
				.StartActive(finishSpanOnDispose: true))
			{
				var count = 0;

				do
				{

					_logger.LogInformation("Current DB connection state: {0:F}", _connection.State);

					if (Connected)
					{
						_logger.LogInformation("Connected to DB after {0:D} attempt(s)", count);
						return;
					}

					_logger.LogInformation("Connecting to DB: attempt {0:D}", count);
					scope.Span.Log(new Dictionary<string, object>(1) { { "attempt" + (count + 1), _connection.State }, });

					_connection.Open();

					System.Threading.Thread.Sleep(millisecondsTimeout: 3_000);
				}
				while (++count <= 10);

				var exception = new Exception("Failed to connect to DB");
				_logger.LogCritical(exception, exception.Message);
				scope.Span.Log(new Dictionary<string, object>(1) { { nameof(exception), exception }, });
				throw exception;
			}
		}

		private Task Deploy()
		{
			_logger.LogInformation("Deploying DB");

			return Task.WhenAll(
				DeployTable("cinema", Properties.Resources.cinema),
				DeployTable("film", Properties.Resources.film),
				DeployTable("show", Properties.Resources.show));
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
