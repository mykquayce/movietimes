using Dawn;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories.Concrete
{
	public class QueriesRepository : Helpers.MySql.RepositoryBase, IQueriesRepository
	{
		public QueriesRepository(
			IOptions<Helpers.MySql.Models.DbSettings> options)
			: base(options.Value)
		{ }

		public async IAsyncEnumerable<Models.QueryResults> GetLastTwoQueryResultsCollectionsAsync(short queryId)
		{
			Guard.Argument(() => queryId).Positive();

			var sql = @"WITH `cte` AS
						(
							SELECT `queryId`
							FROM `queries`.`result`
							WHERE `queryId` = @queryId
								AND `datetime` > DATE_ADD(UTC_TIMESTAMP(), INTERVAL -1 MINUTE)
						)
						SELECT `datetime`, `json`, `checksum`
						FROM `cte`
							JOIN `queries`.`result` r ON `cte`.`queryId` = r.`queryId`
						ORDER BY `datetime` desc
						LIMIT 2;";

			var asyncEnumerable = base.QueryAsync<(DateTime, string, int)>(sql, new { queryId, });

			await foreach (var (dateTime, json, checksum) in asyncEnumerable)
			{
				yield return new Models.QueryResults
				{
					Checksum = checksum,
					DateTime = dateTime,
					Json = json,
					QueryId = queryId,
				};
			}
		}

		public IAsyncEnumerable<(short, string)> GetQueriesAsync()
			=> base.QueryAsync<(short, string)>("SELECT `id`, `query` FROM `queries`.`saved`;");

		public Task SaveQueryResultsAsync(Models.QueryResults queryResults)
		{
			Guard.Argument(() => queryResults).NotEmpty().DoesNotContainNull();
			Guard.Argument(() => queryResults.Checksum).NotDefault();
			Guard.Argument(() => queryResults.Json!).NotNull().NotEmpty().NotWhiteSpace().StartsWith("[");
			Guard.Argument(() => queryResults.QueryId).NotNull().Positive();

			var sql = @"INSERT IGNORE INTO `queries`.`result` (`datetime`, `queryId`, `json`, `checksum`)
						VALUES (@dateTime, @queryId, @json, @checksum);";

			var param = new
			{
				dateTime = DateTime.UtcNow,
				queryResults.QueryId,
				queryResults.Json,
				queryResults.Checksum,
			};

			return base.ExecuteAsync(sql, param);
		}
	}
}
