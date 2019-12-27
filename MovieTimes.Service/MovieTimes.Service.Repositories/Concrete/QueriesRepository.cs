using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories.Concrete
{
	public class QueriesRepository : Helpers.MySql.RepositoryBase, IQueriesRepository
	{
		public QueriesRepository(IOptions<Helpers.MySql.Models.DbSettings> options)
			: base(options.Value)
		{ }

		public IAsyncEnumerable<string> GetLastTwoResultsAsync(short queryId)
			=> base.QueryAsync<string>(
				"SELECT `json` FROM `queries`.`result` WHERE `queryId` = @queryId ORDER BY `datetime` DESC LIMIT 2;",
				new { queryId, });

		public IAsyncEnumerable<(short, string)> GetQueriesAsync()
			=> base.QueryAsync<(short, string)>("SELECT `id`, `query` FROM `queries`.`saved`;");

		public Task SaveQueryResult(short id, string json)
			=> base.ExecuteAsync(
				@"INSERT IGNORE INTO `queries`.`result` (`datetime`, `queryId`, `json`) VALUES (@datetime, @queryId, @json);",
				new { datetime = DateTime.UtcNow, queryId = id, json, });
	}
}
