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

		public IAsyncEnumerable<(int id, string query)> GetQueriesAsync()
			=> base.QueryAsync<(int, string)>("SELECT * FROM queries.saved;");

		public Task SaveQueryResult(int id, string json)
			=> base.ExecuteAsync(
				@"INSERT IGNORE INTO `queries`.`result` (`datetime`, `queryId`, `json`) VALUES (@datetime, @queryId, @json);",
				new { datetime = DateTime.UtcNow, queryId = id, json, });
	}
}
