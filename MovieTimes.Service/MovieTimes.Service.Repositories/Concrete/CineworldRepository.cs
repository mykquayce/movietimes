using Dawn;
using Helpers.Cineworld.Models;
using Helpers.Cineworld.Models.Generated;
using Helpers.Cineworld.Models.Enums;
using Microsoft.Extensions.Options;
using MovieTimes.Service.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			var sql = "SELECT `lastModified` FROM `cineworld`.`log` ORDER BY `timeStamp` DESC LIMIT 1;";

			await foreach (var logEntry in base.QueryAsync<LogEntry>(sql))
			{
				return logEntry;
			}

			return default;
		}

		public Task LogAsync(LogEntry logEntry)
		{
			Guard.Argument(() => logEntry).NotNull();
			Guard.Argument(() => logEntry.LastModified)
				.NotNull()
				.Require(dt => dt.Kind == DateTimeKind.Utc, _ => nameof(LogEntry.LastModified) + " must be UTC");

			return base.ExecuteAsync(
				"INSERT `cineworld`.`log` (`lastModified`) VALUES (@lastModified);",
				logEntry);
		}

		public Task PurgeOldLogsAsync(DateTime? max = default) =>
			base.ExecuteAsync("DELETE FROM `cineworld`.`log` WHERE `lastModified` <= @max;", new { max = max ?? DateTime.UtcNow, });

		public Task PurgeOldShowsAsync(DateTime? max = null) =>
			base.ExecuteAsync("DELETE FROM `cineworld`.`show` WHERE `time` <= @max;", new { max = max ?? DateTime.UtcNow, });

		public IAsyncEnumerable<QueryResult> GetCinemaMovieShowAsync(ICollection<short> cinemaIds)
		{
			Guard.Argument(() => cinemaIds).NotNull();

			var sb = new StringBuilder();

			sb.Append(@"select
					c.id cinemaId,
					c.name cinemaName,
					f.title filmTitle,
					f.length filmLength,
					s.time showDateTime
				from `cineworld`.`show` s
					join `cineworld`.`cinema` c on s.cinemaId = c.id
					join `cineworld`.`film` f on s.filmEdi = f.edi");

			if (cinemaIds.Count > 0)
			{
				sb.Append(" where c.id in (@cinemaIds)");
			}

			sb.Append(" order by c.id, s.time, f.title;");

			var sql = sb.ToString();

			return base.QueryAsync<QueryResult>(sql, new { cinemaIds, });
		}

		public async IAsyncEnumerable<QueryResult> RunQueryAsync(Query query)
		{
			Guard.Argument(() => query).NotNull();
			Guard.Argument(() => query.CinemaIds).NotNull();
			Guard.Argument(() => query.Titles).NotNull();

			var today = DateTime.UtcNow.Date;

			var asyncEnumerable = GetCinemaMovieShowAsync(query.CinemaIds);

			await foreach (var result in asyncEnumerable)
			{
				if (query.CinemaIds.Count > 0 && !query.CinemaIds.Contains(result.CinemaId!.Value))
				{
					continue;
				}

				var daysOfWeek = result.ShowDateTime!.Value.DayOfWeek!.ToDaysOfWeek();

				if ((query.DaysOfWeek & daysOfWeek) == 0)
				{
					continue;
				}

				var hours = query.TimesOfDay.ToHours();

				if (!hours.Contains((byte)result.ShowDateTime!.Value.Hour))
				{
					continue;
				}

				if (query.Titles.Count > 0 && query.Titles.All(s => result.FilmTitle!.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) < 0))
				{
					continue;
				}

				if (result.ShowDateTime < today)
				{
					continue;
				}

				var weeks = (result.ShowDateTime!.Value - today).TotalDays / 7d;

				if (query.WeekCount < weeks)
				{
					continue;
				}

				yield return result;
			}
		}

		public async Task SaveCinemasAsync(ICollection<CinemaType> cinemas)
		{
			Guard.Argument(() => cinemas).NotNull().NotEmpty().DoesNotContainNull();

			using var transaction = base.BeginTransaction();

			try
			{
				await Task.WhenAll(
					base.ExecuteAsync(
						"INSERT IGNORE INTO `cineworld`.`cinema` (`id`, `name`) VALUES (@id, @name);",
						from c in cinemas
						select new
						{
							c.Id,
							c.Name,
						},
						transaction: transaction),
					base.ExecuteAsync(
						"INSERT IGNORE INTO `cineworld`.`film` (`edi`, `title`) VALUES (@edi, @title)",
						from c in cinemas
						from f in c.Films
						where f.Edi > 0 // Theatre Let
						group f by f.Edi into gg
						let first = gg.First()
						select new
						{
							edi = gg.Key,
							title = first.ToString(),
						},
						transaction: transaction)
				);

				await base.ExecuteAsync(
					"INSERT IGNORE INTO `cineworld`.`show` (`cinemaId`, `filmEdi`, `time`) VALUES (@cinemaId, @filmEdi, @time)",
					from c in cinemas
					from f in c.Films
					where f.Edi > 0 // Theatre Let
					from dt in f.DateTimes
					group (c, f, dt) by (c.Id, f.Edi, dt) into gg
					select new
					{
						cinemaId = gg.Key.Id,
						filmEdi = gg.Key.Edi,
						time = gg.Key.dt,
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

		public Task SaveLengthsAsync(ICollection<FilmType> films)
		{
			Guard.Argument(() => films).NotNull().NotEmpty().DoesNotContainNull();

			var sql = "UPDATE `cineworld`.`film` SET `length` = @length WHERE `edi` = @edi;";

			return base.ExecuteAsync(sql, films);
		}
	}
}
